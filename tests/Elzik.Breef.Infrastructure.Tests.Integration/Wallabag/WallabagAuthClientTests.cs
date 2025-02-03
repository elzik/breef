using Elzik.Breef.Infrastructure.Wallabag;
using Refit;
using Shouldly;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration.Wallabag
{
    public class WallabagAuthClientTests
    {
        private readonly IWallabagAuthClient? _wallabagAuthClient;
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly string? _wallabagUrl;
        private readonly string? _wallaClientId;
        private readonly string? _wallabagClientSecret;
        private readonly string? _wallabagUsername;
        private readonly string? _wallabagPassword;

        public WallabagAuthClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper
                ?? throw new ArgumentNullException(nameof(testOutputHelper));

            _wallabagUrl = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_URL");
            _wallaClientId = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_CLIENT_ID");
            _wallabagClientSecret = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_CLIENT_SECRET");
            _wallabagUsername = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_USERNAME");
            _wallabagPassword = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_PASSWORD");
            if (string.IsNullOrWhiteSpace(_wallabagUrl)
                || string.IsNullOrWhiteSpace(_wallaClientId)
                || string.IsNullOrWhiteSpace(_wallabagClientSecret)
                || string.IsNullOrWhiteSpace(_wallabagUsername)
                || string.IsNullOrWhiteSpace(_wallabagPassword))
            {
                return;
            }

            var refitSettings = new RefitSettings();
            if (Environment.GetEnvironmentVariable("BREEF_TESTS_ENABLE_HTTP_MESSAGE_LOGGING") == "true")
            {
                refitSettings.HttpMessageHandlerFactory = () => new HttpMessageXunitLoggingHandler(_testOutputHelper);
            }

            _wallabagAuthClient = RestService.For<IWallabagAuthClient>(_wallabagUrl, refitSettings);
            if (_wallabagAuthClient == null)
            {
                throw new InvalidOperationException("Failed to create Wallabag client.");
            }
        }

        [SkippableFact]
        public async Task GetTokenAsync_FromTestWallabagAccount_ReturnsToken()
        {
            SkipIfEnvironmentVariablesNotSet();

            // Arrange
            var testWallabagTokenRequest = new TokenRequest
            {
                // Forgive nulls as they will cause the test to be skipped anyway
                ClientId = _wallaClientId!,
                ClientSecret = _wallabagClientSecret!,
                Username = _wallabagUsername!,
                Password = _wallabagPassword!
            };

            // Act
            var tokenResponse = await _wallabagAuthClient!.GetTokenAsync(testWallabagTokenRequest!); // ctor will fail if _wallabagTokenRequest is null

            // Assert
            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNullOrEmpty();
            tokenResponse.RefreshToken.ShouldNotBeNullOrEmpty();
            tokenResponse.TokenType.ShouldBe("bearer");
        }

        private void SkipIfEnvironmentVariablesNotSet()
        {
            Skip.If(string.IsNullOrWhiteSpace(_wallabagUrl),
                "Skipped because no URL provided in BREEF_TESTS_WALLABAG_URL env. variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallaClientId),
                "Skipped because no client ID provided in BREEF_TESTS_WALLABAG_CLIENT_ID env. variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallabagClientSecret),
                "Skipped because no client secret provided in BREEF_TESTS_WALLABAG_CLIENT_SECRET env. variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallabagUsername),
                "Skipped because no username provided in BREEF_TESTS_WALLABAG_USERNAME env. variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallabagPassword),
                "Skipped because no password provided in BREEF_TESTS_WALLABAG_PASSWORD env. variable.");
        }
    }
}
