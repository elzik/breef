using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Elzik.Breef.Api.Tests.Functional;

public class HealthTestsDocker : HealthTestsBase, IAsyncLifetime
{
    private const string DockerImageName = "ghcr.io/elzik/elzik-breef-api:latest";
    private const int ContainerStartTimeoutSeconds = 30;
    private readonly IContainer? _testContainer;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly bool _dockerIsUnavailable;

    private readonly HttpClient _client;

    protected override HttpClient Client => _client;
    protected override bool SkipTestsIf => _dockerIsUnavailable;
    protected override string SkipTestsReason => "Test was skipped because Docker is not available. Install Docker Engine for Linux, " +
        "Docker Desktop for Windows or make peace with the fact that tests can not run for the Docker container.";

    public HealthTestsDocker(ITestOutputHelper testOutputHelper)
    {
        _dockerIsUnavailable = DockerIsUnavailable();
        _testOutputHelper = testOutputHelper
 ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        if (!_dockerIsUnavailable)
        {
            BuildDockerImage();

            var outputConsumer = Consume.RedirectStdoutAndStderrToStream(
                new TestOutputHelperStream(_testOutputHelper),
                new TestOutputHelperStream(_testOutputHelper));

            _testContainer = new ContainerBuilder()
                .WithImage(DockerImageName)
                .WithPortBinding(8080, true)
                .WithEnvironment("breef_BreefApi__ApiKey", "dummy-api-key")
                .WithEnvironment("breef_AiService__Provider", "OpenAI")
                .WithEnvironment("breef_AiService__EndpointUrl", "http://dummy-ai.local")
                .WithEnvironment("breef_AiService__ModelId", "dummy-model")
                .WithEnvironment("breef_AiService__ApiKey", "dummy-api-key")
                .WithEnvironment("breef_Wallabag__BaseUrl", "http://dummy-wallabag.local")
                .WithEnvironment("breef_Wallabag__Username", "dummy-user")
                .WithEnvironment("breef_Wallabag__Password", "dummy-password")
                .WithEnvironment("breef_Wallabag__ClientId", "dummy-client-id")
                .WithEnvironment("breef_Wallabag__ClientSecret", "dummy-client-secret")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request
                .ForPort(8080)
                .ForPath("/health")
                .ForStatusCode(System.Net.HttpStatusCode.OK)))
                .WithOutputConsumer(outputConsumer)
                .Build();
        }
    }

    private void BuildDockerImage()
    {
        var dockerScriptPath = Path.GetFullPath("./../../../../../build/api/build-docker.ps1");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{dockerScriptPath}\" -Version latest",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };

        process.Start();
        process.WaitForExit();

        string standardOutput = process.StandardOutput.ReadToEnd();
        string standardError = process.StandardError.ReadToEnd();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
             standardError + Environment.NewLine + standardOutput);
        }

        _testOutputHelper.WriteLine(standardOutput);
    }

    private static bool DockerIsUnavailable()
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
            process.WaitForExit();

            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                   standardError + Environment.NewLine + standardOutput);
            }

            return !standardOutput.StartsWith("Docker version", StringComparison.OrdinalIgnoreCase);
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            if (ex.Message.StartsWith("An error occurred trying to start process")
                && ex.Message.EndsWith("The system cannot find the file specified."))
            {
                return true;
            }

            throw;
        }
    }

    public async Task InitializeAsync()
    {
        if (!_dockerIsUnavailable)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ContainerStartTimeoutSeconds));

            if (_testContainer == null)
            {
                throw new InvalidOperationException("Test container is not initialized and cannot be started.");
            }

            try
            {
                await _testContainer.StartAsync(timeoutCts.Token);
                HostPort = _testContainer.GetMappedPublicPort(8080);
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"Container failed to start within {ContainerStartTimeoutSeconds} seconds. " +
                                           $"This may indicate that the container is taking too long to become ready " +
                                           $"or there's an issue with the container startup.");
            }
        }
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();

        if (!_dockerIsUnavailable && _testContainer != null)
        {
            await _testContainer.StopAsync();
            await _testContainer.DisposeAsync();
        }
    }
}
