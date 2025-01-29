using Elzik.Breef.Infrastructure.Wallabag;
using Refit;
using Shouldly;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration.Wallabag
{
    public class WallabagClientTests
    {
        private readonly IWallabagClient? _wallabagClient;
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly string? _wallabagUrl;
        private readonly string? _wallaClientId;
        private readonly string? _wallabagClientSecret;
        private readonly string? _wallabagUsername;
        private readonly string? _wallabagPassword;
        private readonly TokenRequest? _wallabagTokenRequest;

        public WallabagClientTests(ITestOutputHelper testOutputHelper)
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
            _wallabagTokenRequest = new TokenRequest
            {
                ClientId = _wallaClientId,
                ClientSecret = _wallabagClientSecret,
                Username = _wallabagUsername,
                Password = _wallabagPassword
            };
            if (_wallabagTokenRequest == null)
            {
                throw new InvalidOperationException("Failed to create Wallabag client.");
            }


            var refitSettings = new RefitSettings
            {
                AuthorizationHeaderValueGetter = async (request, cancellationToken) =>
                {

                    var tokenResponse = await _wallabagClient!.GetTokenAsync(_wallabagTokenRequest); // _wallabagClient will only be null if test is skipped
                    return tokenResponse.AccessToken;
                }
            };

            if (Environment.GetEnvironmentVariable("BREEF_TESTS_ENABLE_HTTP_MESSAGE_LOGGING") == "true")
            {
                refitSettings.HttpMessageHandlerFactory = () => new HttpMessageLoggingHandler(_testOutputHelper);
            }

            _wallabagClient = RestService.For<IWallabagClient>(_wallabagUrl, refitSettings);
            if (_wallabagClient == null)
            {
                throw new InvalidOperationException("Failed to create Wallabag client.");
            }
        }

        [SkippableFact]
        public async Task GetTokenAsync_FromTestWallabagAccount_ReturnsToken()
        {
            SkipIfEnvironmentVariablesNotSet();

            // Act
            var tokenResponse = await _wallabagClient!.GetTokenAsync(_wallabagTokenRequest!); // ctor will fail if _wallabagTokenRequest is null
            // Assert
            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNullOrEmpty();
            tokenResponse.RefreshToken.ShouldNotBeNullOrEmpty();
            tokenResponse.TokenType.ShouldBe("bearer");
        }

        [SkippableFact]
        public async Task PostEntryAsync_WithValidEntry_PostsEntry()
        {
            SkipIfEnvironmentVariablesNotSet();

            // Arrange
            var entry = new WallabagEntryCreateRequest
            {
                Url = $"https://www.{Guid.NewGuid()}.com",
                Title = $"Example-{Guid.NewGuid()}",
                Content = "Example content",
                Tags = "example"
            };

            // Act
            var wallabagEntry = await _wallabagClient!.PostEntryAsync(entry); // _wallabagClient will only be null if test is skipped

            // Assert
            wallabagEntry.ShouldNotBeNull();
            wallabagEntry.Links.Self.ShouldNotBeNull();
            wallabagEntry.Links.Self.Href.ShouldNotBeNullOrEmpty();
            wallabagEntry.Tags.ShouldHaveSingleItem();
            wallabagEntry.Tags[0].Label.ShouldBe(entry.Tags);
            wallabagEntry.Title.ShouldBe(entry.Title);
            wallabagEntry.Url.ShouldBe(entry.Url);
            wallabagEntry.Content.ShouldBe(entry.Content);
            var now = DateTime.Now;
            var thirtySecondsAgo = now.AddSeconds(-30);
            var thirtySecondsFromNow = now.AddSeconds(-30);
            wallabagEntry.CreatedAt.ShouldNotBeInRange(thirtySecondsAgo, thirtySecondsFromNow);
            wallabagEntry.UpdatedAt.ShouldNotBeInRange(thirtySecondsAgo, thirtySecondsFromNow);
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
