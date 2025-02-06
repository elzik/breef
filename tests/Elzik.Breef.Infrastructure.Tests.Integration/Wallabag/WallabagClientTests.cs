using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Configuration;
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

        public WallabagClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper
                ?? throw new ArgumentNullException(nameof(testOutputHelper));

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("breef_")
                .Build();
            var wallabagOptions = configuration.GetRequiredSection("Wallabag").Get<WallabagOptions>()
                ?? throw new InvalidOperationException("Wallabag options not found in Wallabag configuration section.");

            var wallabagTokenRequest = new TokenRequest
            {
                ClientId = wallabagOptions.ClientId,
                ClientSecret = wallabagOptions.ClientSecret,
                Username = wallabagOptions.Username,
                Password = wallabagOptions.Password
            };

            var wallabagAuthClient = RestService.For<IWallabagAuthClient>(wallabagOptions.BaseUrl);
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

            _wallabagClient = RestService.For<IWallabagClient>(wallabagOptions.BaseUrl, refitSettings);
        }

        [Theory]
        [InlineData("https://wallabag.elzik.co.uk/img/logo-wallabag.svg")]
        [InlineData(null)]
        public async Task PostEntryAsync_WithValidEntry_PostsEntry(string? previewImageUrl)
        {
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
    }
}
