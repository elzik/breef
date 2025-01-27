using Refit;
using Elzik.Breef.Infrastructure.Wallabag;
using FluentAssertions;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration.Wallabag
{
    public class WallabagClientTests
    {
        private readonly IWallabagClient _wallabagClient;
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly string? _wallabagUrl;
        private readonly string? _wallaClientId;
        private readonly string? _wallabagClientSecret;
        private readonly string? _wallabagUsername;
        private readonly string? _wallabagPassword;

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

            var refitSettings = new RefitSettings
            {
                AuthorizationHeaderValueGetter = async (request, cancellationToken) =>
                {
                    var tokenRequest = new TokenRequest
                    {
                        ClientId = _wallaClientId,
                        ClientSecret = _wallabagClientSecret,
                        Username = _wallabagUsername,
                        Password = _wallabagPassword
                    };

                    var tokenResponse = await _wallabagClient!.GetTokenAsync(tokenRequest);
                    return tokenResponse.AccessToken;
                }
            };

            if(Environment.GetEnvironmentVariable("BREEF_TESTS_ENABLE_HTTP_MESSAGE_LOGGING") == "true")
            {
                refitSettings.HttpMessageHandlerFactory = () => new HttpMessageLoggingHandler(_testOutputHelper);
            }

            _wallabagClient = RestService.For<IWallabagClient>(_wallabagUrl, refitSettings);
        }

        [SkippableFact]
        public async Task GetTokenAsync_FromTestWallabagAccount_ReturnsToken()
        {
            SkipIfEnvironmentVariablesNotSet();

            // Arrange
            var tokenRequest = new TokenRequest
            {
                ClientId = _wallaClientId,
                ClientSecret = _wallabagClientSecret,
                Username = _wallabagUsername,
                Password = _wallabagPassword
            };

            // Act
            var tokenResponse = await _wallabagClient.GetTokenAsync(tokenRequest);

            // Assert
            tokenResponse.Should().NotBeNull();
            tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
            tokenResponse.RefreshToken.Should().NotBeNullOrEmpty();
            tokenResponse.TokenType.Should().Be("bearer");
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
            var wallabagEntry = await _wallabagClient.PostEntryAsync(entry);
            
            // Assert
            wallabagEntry.Should().NotBeNull();
            wallabagEntry.Links.Self.Should().NotBeNull();
            wallabagEntry.Links.Self.Href.Should().NotBeNullOrEmpty();
            wallabagEntry.Tags.Should().ContainSingle();
            wallabagEntry.Tags[0].Label.Should().Be(entry.Tags);
            wallabagEntry.Title.Should().Be(entry.Title);
            wallabagEntry.Url.Should().Be(entry.Url);
            wallabagEntry.Content.Should().Be(entry.Content);
            wallabagEntry.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
            wallabagEntry.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
        }

        private void SkipIfEnvironmentVariablesNotSet()
        {
            Skip.If(string.IsNullOrWhiteSpace(_wallabagUrl), "Skipped because no Wallabag URL provided in BREEF_TESTS_WALLABAG_URL environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallaClientId), "Skipped because no Wallabag client ID provided in BREEF_TESTS_WALLABAG_CLIENT_ID environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallabagClientSecret), "Skipped because no Wallabag client secret provided in BREEF_TESTS_WALLABAG_CLIENT_SECRET environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallabagUsername), "Skipped because no Wallabag username provided in BREEF_TESTS_WALLABAG_USERNAME environment variable.");
            Skip.If(string.IsNullOrWhiteSpace(_wallabagPassword), "Skipped because no Wallabag password provided in BREEF_TESTS_WALLABAG_PASSWORD environment variable.");
        }
    }
}
