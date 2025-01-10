using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Diagnostics;
using System.Management.Automation;
using Xunit.Abstractions;

namespace Elzik.Breef.Api.Tests.Functional;

public class BreefTestsDocker : BreefTestsBase, IAsyncLifetime
{
    private const string DockerImageName = "ghcr.io/elzik/elzik-breef-api:latest";
    private readonly IContainer _testContainer;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly bool _skipTestsIf;

    private readonly HttpClient _client;

    public override HttpClient Client => _client;
    protected override bool SkipTestsIf => _skipTestsIf;
    protected override string SkipTestsReason => "Docker is not available. Install Docker Engine for Linux, " +
        "Docker Desktop for Windows or make peace with the fact that tests can not run for the Docker container.";

    public BreefTestsDocker(ITestOutputHelper testOutputHelper)
    {
        _skipTestsIf = DockerIsUnavailable();

        if (!_skipTestsIf)
        {
            _testOutputHelper = testOutputHelper
                ?? throw new ArgumentNullException(nameof(testOutputHelper));

            BuildDockerImage();

            var outputConsumer = Consume.RedirectStdoutAndStderrToStream(
                        new TestOutputHelperStream(_testOutputHelper),
                        new TestOutputHelperStream(_testOutputHelper));

            _testContainer = new ContainerBuilder()
                .WithImage(DockerImageName) // Replace with your Docker image
                .WithPortBinding(8080, true) // Use a random available port on the host
                .WithEnvironment("BREEF_API_KEY", ApiKey)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
                .WithOutputConsumer(outputConsumer)
                .Build();

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("BREEF-API-KEY", ApiKey);
        }
    }

    private void BuildDockerImage()
    {
        var dockerScriptPath = Path.GetFullPath("./../../../../../build/api/build-docker.ps1");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{dockerScriptPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                process.StandardError.ReadToEnd() + Environment.NewLine + process.StandardOutput.ReadToEnd());
        }

        _testOutputHelper.WriteLine(process.StandardOutput.ReadToEnd());
    }

    private bool DockerIsUnavailable()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    process.StandardError.ReadToEnd() + Environment.NewLine + process.StandardOutput.ReadToEnd());
            }

            return !output.StartsWith("Docker version", StringComparison.OrdinalIgnoreCase);
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            if(ex.Message.StartsWith("An error occurred trying to start process") 
                && ex.Message.EndsWith("The system cannot find the file specified."))
            {
                return true;
            }

            throw;
        }
    }

    public async Task InitializeAsync()
    {
        if (!_skipTestsIf)
        {
            await _testContainer.StartAsync();
            HostPort = _testContainer.GetMappedPublicPort(8080);
        }
    }

    public async Task DisposeAsync()
    {
        if (!_skipTestsIf)
        {
            await _testContainer.StopAsync();
        }
    }
}

public class TestOutputHelperStream : Stream
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly MemoryStream _memoryStream = new();

    public TestOutputHelperStream(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _memoryStream.Write(buffer, offset, count);
        _testOutputHelper.WriteLine(System.Text.Encoding.UTF8.GetString(buffer, offset, count));
    }

    // Other required Stream members
    public override void Flush() => _memoryStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _memoryStream.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _memoryStream.Seek(offset, origin);
    public override void SetLength(long value) => _memoryStream.SetLength(value);
    public override bool CanRead => _memoryStream.CanRead;
    public override bool CanSeek => _memoryStream.CanSeek;
    public override bool CanWrite => _memoryStream.CanWrite;
    public override long Length => _memoryStream.Length;
    public override long Position { get => _memoryStream.Position; set => _memoryStream.Position = value; }
}