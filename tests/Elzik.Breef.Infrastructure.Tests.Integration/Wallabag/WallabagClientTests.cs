using Elzik.Breef.Infrastructure.Wallabag;
using Newtonsoft.Json;
using Refit;
using Shouldly;
using System.Diagnostics;
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
            var wallabagTokenRequest = new TokenRequest
            {
                ClientId = _wallaClientId,
                ClientSecret = _wallabagClientSecret,
                Username = _wallabagUsername,
                Password = _wallabagPassword
            };

            var wallabagAuthClient = RestService.For<IWallabagAuthClient>(_wallabagUrl);
            var refitSettings = new RefitSettings
            {
                AuthorizationHeaderValueGetter = async (request, cancellationToken) =>
                {
                    var tokenResponse = await wallabagAuthClient.GetTokenAsync(wallabagTokenRequest);
                    return tokenResponse.AccessToken;
                }
            };

            if (Environment.GetEnvironmentVariable("BREEF_TESTS_ENABLE_HTTP_MESSAGE_LOGGING") == "true")
            {
                refitSettings.HttpMessageHandlerFactory = () => new HttpMessageXunitLoggingHandler(_testOutputHelper);
            }

            _wallabagClient = RestService.For<IWallabagClient>(_wallabagUrl, refitSettings);
        }

        [SkippableTheory]
        [InlineData("https://wallabag.elzik.co.uk/img/logo-wallabag.svg")]
        [InlineData(null)]
        public async Task PostEntryAsync_WithValidEntry_PostsEntry(string? previewImageUrl)
        {
            SkipIfEnvironmentVariablesNotSet();

            // Arrange
            var entry = new WallabagEntryCreateRequest
            {
                Url = $"https://www.{Guid.NewGuid()}.com",
                Title = $"Example-{DateTime.Now:yyyy-MM-ddTHH-mm-ss.fff}",
                Content = "Example content",
                Tags = "example",
                PreviewPicture = previewImageUrl
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
            wallabagEntry.PreviewPicture.ShouldBe(entry.PreviewPicture);
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
