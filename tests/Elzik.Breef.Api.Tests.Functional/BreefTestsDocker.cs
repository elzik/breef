using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Elzik.Breef.Api.Tests.Functional;

public class BreefTestsDocker : BreefTestsBase, IAsyncLifetime
{
    private const string DockerImageName = "ghcr.io/elzik/elzik-breef-api:latest";
    private readonly IContainer? _testContainer;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly bool _dockerIsUnavailable;

    private readonly HttpClient _client;

    protected override HttpClient Client => _client;
    protected override bool SkipTestsIf => _dockerIsUnavailable;
    protected override string SkipTestsReason => "Test was skipped because Docker is not available. Install Docker Engine for Linux, " +
        "Docker Desktop for Windows or make peace with the fact that tests can not run for the Docker container.";

    public BreefTestsDocker(ITestOutputHelper testOutputHelper)
    {
        _dockerIsUnavailable = DockerIsUnavailable();
        _testOutputHelper = testOutputHelper
            ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("BREEF-API-KEY", ApiKey);

        if (!_dockerIsUnavailable)
        {
            BuildDockerImage();

            var modelId = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_MODEL_ID");
            Skip.If(string.IsNullOrWhiteSpace(modelId),
                "Skipped because no AI model ID provided in BREEF_TESTS_AI_MODEL_ID environment variable.");
            var endpoint = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_ENDPOINT");
            Skip.If(string.IsNullOrWhiteSpace(endpoint),
                "Skipped because no AI endpoint provided in BREEF_TESTS_AI_ENDPOINT environment variable.");
            var apiKey = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_API_KEY");
            Skip.If(string.IsNullOrWhiteSpace(apiKey),
                "Skipped because no AI API key provided in BREEF_TESTS_AI_API_KEY environment variable.");

            var outputConsumer = Consume.RedirectStdoutAndStderrToStream(
                        new TestOutputHelperStream(_testOutputHelper),
                        new TestOutputHelperStream(_testOutputHelper));

            _testContainer = new ContainerBuilder()
                .WithImage(DockerImageName)
                .WithPortBinding(8080, true)
                .WithEnvironment("BREEF_API_KEY", ApiKey)
                .WithEnvironment("BREEF_TESTS_AI_MODEL_ID", modelId)
                .WithEnvironment("BREEF_TESTS_AI_ENDPOINT", endpoint)
                .WithEnvironment("BREEF_TESTS_AI_API_KEY", apiKey)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
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
            await _testContainer!.StartAsync(); // Null forgiven since if we're not skipping tests,
                                                // _testContainer will never be null
            HostPort = _testContainer.GetMappedPublicPort(8080);
        }
    }

    public async Task DisposeAsync()
    {
        if (!_dockerIsUnavailable)
        {
            await _testContainer!.StopAsync(); // Null forgiven since if we're not skipping tests,
                                               // _testContainer will never be null
        }
    }
}
