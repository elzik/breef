using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Elzik.Breef.Api.Tests.Functional;

public class BreefTestsDocker : BreefTestsBase, IAsyncLifetime
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

            string? breefAiServiceProvider = Environment.GetEnvironmentVariable("breef_AiService__Provider");
            string? breefAiServiceEndpointUrl = Environment.GetEnvironmentVariable("breef_AiService__EndpointUrl");
            string? breefAiServiceModelId = Environment.GetEnvironmentVariable("breef_AiService__ModelId");
            string? breefAiServiceApiKey = Environment.GetEnvironmentVariable("breef_AiService__ApiKey");

            string? breefWallabagBaseUrl = Environment.GetEnvironmentVariable("breef_Wallabag__BaseUrl");
            string? breefWallabagUsername = Environment.GetEnvironmentVariable("breef_Wallabag__Username");
            string? breefWallabagPassword = Environment.GetEnvironmentVariable("breef_Wallabag__Password");
            string? breefWallabagClientId = Environment.GetEnvironmentVariable("breef_Wallabag__ClientId");
            string? breefWallabagClientSecret = Environment.GetEnvironmentVariable("breef_Wallabag__ClientSecret");

            Skip.If(string.IsNullOrWhiteSpace(breefAiServiceProvider),
                "Skipped because no AI service provider provided in breef_AiService__Provider environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(breefAiServiceEndpointUrl),
                "Skipped because no AI endpoint provided in breef_AiService__EndpointUrl environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(breefAiServiceModelId),
                "Skipped because no AI model ID provided in breef_AiService__ModelId environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(breefAiServiceApiKey),
                "Skipped because no AI API key provided in breef_AiService__ApiKey environment variable.");

            Skip.If(string.IsNullOrWhiteSpace(breefWallabagBaseUrl),
                "Skipped because no Wallabag URL provided in breef_Wallabag__BaseUrl environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(breefWallabagUsername),
                "Skipped because no Wallabag username provided in breef_Wallabag__Username environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(breefWallabagPassword),
                "Skipped because no Wallabag password provided in breef_Wallabag__Password environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(breefWallabagClientId),
                "Skipped because no Wallabag client ID provided in breef_Wallabag__ClientId environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(breefWallabagClientSecret),
                "Skipped because no Wallabag client secret provided in breef_Wallabag__ClientSecret environment variable.");

            var outputConsumer = Consume.RedirectStdoutAndStderrToStream(
                        new TestOutputHelperStream(_testOutputHelper),
                        new TestOutputHelperStream(_testOutputHelper));

            _testContainer = new ContainerBuilder()
                .WithImage(DockerImageName)
                .WithPortBinding(8080, true)
                .WithEnvironment("breef_BreefApi__ApiKey", ApiKey)
                .WithEnvironment("breef_AiService__Provider", breefAiServiceProvider)
                .WithEnvironment("breef_AiService__EndpointUrl", breefAiServiceEndpointUrl)
                .WithEnvironment("breef_AiService__ModelId", breefAiServiceModelId)
                .WithEnvironment("breef_AiService__ApiKey", breefAiServiceApiKey)
                .WithEnvironment("breef_Wallabag__BaseUrl", breefWallabagBaseUrl)
                .WithEnvironment("breef_Wallabag__Username", breefWallabagUsername)
                .WithEnvironment("breef_Wallabag__Password", breefWallabagPassword)
                .WithEnvironment("breef_Wallabag__ClientId", breefWallabagClientId)
                .WithEnvironment("breef_Wallabag__ClientSecret", breefWallabagClientSecret)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(8080))
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
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ContainerStartTimeoutSeconds));

            if(_testContainer == null)
            {
                throw new InvalidOperationException("Test container is not initialized " +
                                                    "and cannot be started.");
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
        if (!_dockerIsUnavailable)
        {
            if (_testContainer == null)
            {
                throw new InvalidOperationException("Test container is not initialized " +
                                                    "and cannot be stopped.");
            }

            await _testContainer.StopAsync();
        }
    }
}
