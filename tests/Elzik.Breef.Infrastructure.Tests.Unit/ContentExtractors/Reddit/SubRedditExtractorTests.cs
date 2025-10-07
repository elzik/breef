using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit
{
    public class SubRedditExtractorTests
    {
        private readonly IHttpDownloader _mockHttpDownloader;
        private readonly IOptions<RedditOptions> _mockRedditOptions;
        private readonly SubRedditContentExtractor _extractor;

        public SubRedditExtractorTests()
        {
            _mockHttpDownloader = Substitute.For<IHttpDownloader>();
            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult("<html><body>Mocked content</body></html>"));
            
            _mockRedditOptions = Substitute.For<IOptions<RedditOptions>>();
            _mockRedditOptions.Value.Returns(new RedditOptions());
            
            _extractor = new SubRedditContentExtractor(_mockHttpDownloader, _mockRedditOptions);
        }

        [Theory]
        [InlineData("https://reddit.com/r/testsubreddit/")]
        [InlineData("https://reddit.com/r/testsubreddit")]
        [InlineData("hTTpS://rEDdiT.cOm/R/tEsTsUbReDdIt/")]
        [InlineData("https://www.reddit.com/r/testsubreddit/")]
        public void CanHandle_ValidSubRedditUrl_ReturnsTrue(string url)
        {
            // Act
            var canHandle = _extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeTrue();
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("https://reddit.com")]
        [InlineData("https://reddit.com/r")]
        [InlineData("https://reddit.com/r/testsubreddit/more")]
        [InlineData("https://not-reddit.com/r/testsubreddit/")]
        [InlineData("https://www2.reddit.com/r/testsubreddit/")]
        public void CanHandle_InvalidSubRedditUrl_ReturnsFalse(string url)
        {
            // Act
            var canHandle = _extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeFalse();
        }

        [Theory]
        [InlineData("https://custom.reddit.com/r/testsubreddit/")]
        [InlineData("https://alt.reddit.instance.com/r/testsubreddit/")]
        public void CanHandle_CustomRedditInstance_ReturnsTrue(string url)
        {
            // Arrange
            var customOptions = new RedditOptions
            {
                DefaultBaseAddress = "https://www.reddit.com",
                AdditionalBaseAddresses = ["https://reddit.com", "https://custom.reddit.com", "https://alt.reddit.instance.com"]
            };
            _mockRedditOptions.Value.Returns(customOptions);
            var extractor = new SubRedditContentExtractor(_mockHttpDownloader, _mockRedditOptions);

            // Act
            var canHandle = extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeTrue();
        }

        [Theory]
        [InlineData("https://unknown.reddit.com/r/testsubreddit/")]
        [InlineData("https://www.unknown.reddit.com/r/testsubreddit/")]
        public void CanHandle_UnknownRedditInstance_ReturnsFalse(string url)
        {
            // Arrange
            var customOptions = new RedditOptions
            {
                DefaultBaseAddress = "https://www.reddit.com",
                AdditionalBaseAddresses = ["https://reddit.com", "https://custom.reddit.com"]
            };
            _mockRedditOptions.Value.Returns(customOptions);
            var extractor = new SubRedditContentExtractor(_mockHttpDownloader, _mockRedditOptions);

            // Act
            var canHandle = extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeFalse();
        }

        [Theory]
        [InlineData("icon_img")]
        [InlineData("community_icon")]
        [InlineData("banner_background_image")]
        [InlineData("banner_img")]
        [InlineData("mobile_banner_image")]
        public async Task ExtractAsync_ImageKeyExistsAndIsAccessible_ReturnsImageUrl(string imageKey)
        {
            // Arrange
            var url = $"https://www.reddit.com/r/subreddit";
            var imageUrl = $"https://img.reddit.com/{imageKey}.png";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith(".json")))
                           .Returns(Task.FromResult(json));
            _mockHttpDownloader.TryGet(imageUrl).Returns(true);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(imageUrl);
        }

        [Theory]
        [InlineData("icon_img")]
        [InlineData("community_icon")]
        [InlineData("banner_background_image")]
        [InlineData("banner_img")]
        [InlineData("mobile_banner_image")]
        public async Task ExtractAsync_TryGetReturnsFalse_UsesDefaultImageUrl(string imageKey)
        {
            // Arrange
            var url = $"https://www.reddit.com/r/subreddit";
            var imageUrl = $"https://img.reddit.com/{imageKey}.png";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith(".json")))
                .Returns(Task.FromResult(json));
            _mockHttpDownloader.TryGet(imageUrl).Returns(false);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe("https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg");
        }

        [Fact]
        public async Task ExtractAsync_NoImageKeysExist_UsesDefaultImageUrl()
        {
            // Arrange
            var url = $"https://www.reddit.com/r/subreddit";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith(".json")))
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe("https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg");
        }

        [Fact]
        public async Task ExtractAsync_AvailableContent_ReturnsExpectedTitle()
        {
            // Arrange
            var url = $"https://www.reddit.com/r/subreddit";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith(".json")))
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.Title.ShouldBe($"New in r/subreddit");
        }

        [Fact]
        public async Task ExtractAsync_AvailableContent_ReturnsExpectedContent()
        {
            // Arrange
            var url = $"https://www.reddit.com/r/subreddit";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith(".json")))
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.Content.ShouldBe(json);
        }

        [Theory]
        [InlineData("https://www.reddit.com/r/testsubreddit")]
        [InlineData("https://www.reddit.com/r/testsubreddit/")]
        public async Task ExtractAsync_ValidUrl_CallsHttpDownloaderWithCorrectUrl(string subredditUrl)
        {
            // Arrange
            var expectedApiUrl = "https://www.reddit.com/r/testsubreddit/new.json";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            await _extractor.ExtractAsync(subredditUrl);

            // Assert
            await _mockHttpDownloader.Received(1).DownloadAsync(expectedApiUrl);
        }

        [Theory]
        [InlineData("icon_img")]
        [InlineData("community_icon")]
        [InlineData("banner_background_image")]
        [InlineData("banner_img")]
        [InlineData("mobile_banner_image")]
        public async Task GetSubredditImageUrlAsync_ImageKeyExistsAndIsAccessible_ReturnsImageUrl(string imageKey)
        {
            // Arrange
            var subredditName = "programming";
            var imageUrl = $"https://img.reddit.com/{imageKey}.png";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync($"https://www.reddit.com/r/{subredditName}/about.json")
                .Returns(Task.FromResult(json));
            _mockHttpDownloader.TryGet(imageUrl).Returns(true);

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(imageUrl);
        }

        [Theory]
        [InlineData("programming")]
        [InlineData("learnprogramming")]
        [InlineData("AskReddit")]
        [InlineData("funny")]
        public async Task GetSubredditImageUrlAsync_ValidSubredditName_CallsCorrectAboutUrl(string subredditName)
        {
            // Arrange
            var expectedUrl = $"https://www.reddit.com/r/{subredditName}/about.json";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(expectedUrl)
                .Returns(Task.FromResult(json));

            // Act
            await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            await _mockHttpDownloader.Received(1).DownloadAsync(expectedUrl);
        }

        [Fact]
        public async Task GetSubredditImageUrlAsync_NoImageKeysExist_ReturnsDefaultImageUrl()
        {
            // Arrange
            var subredditName = "programming";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe("https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg");
        }

        [Fact]
        public async Task GetSubredditImageUrlAsync_ImageExistsButNotAccessible_ReturnsDefaultImageUrl()
        {
            // Arrange
            var subredditName = "programming";
            var imageUrl = "https://img.reddit.com/icon.png";
            var json = CreateJsonWithImageKey("icon_img", imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));
            _mockHttpDownloader.TryGet(imageUrl).Returns(false);

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe("https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg");
        }

        [Fact]
        public async Task GetSubredditImageUrlAsync_MultipleImageKeys_ReturnsFirstAccessibleImage()
        {
            // Arrange
            var subredditName = "programming";
            var bannerImageUrl = "https://img.reddit.com/banner.png";
            var iconImageUrl = "https://img.reddit.com/icon.png";
            
            var json = JsonSerializer.Serialize(new
            {
                data = new Dictionary<string, object>
                {
                    { "banner_background_image", bannerImageUrl },
                    { "icon_img", iconImageUrl }
                }
            });

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));
            _mockHttpDownloader.TryGet(bannerImageUrl).Returns(true);
            _mockHttpDownloader.TryGet(iconImageUrl).Returns(true);

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(bannerImageUrl); // Should return the first accessible image based on priority order
        }

        [Fact]
        public async Task GetSubredditImageUrlAsync_FirstImageNotAccessible_ReturnsSecondImage()
        {
            // Arrange
            var subredditName = "programming";
            var bannerImageUrl = "https://img.reddit.com/banner.png";
            var iconImageUrl = "https://img.reddit.com/icon.png";
            
            var json = JsonSerializer.Serialize(new
            {
                data = new Dictionary<string, object>
                {
                    { "banner_background_image", bannerImageUrl },
                    { "icon_img", iconImageUrl }
                }
            });

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));
            _mockHttpDownloader.TryGet(bannerImageUrl).Returns(false);
            _mockHttpDownloader.TryGet(iconImageUrl).Returns(true);

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(iconImageUrl);
        }

        [Fact]
        public async Task GetSubredditImageUrlAsync_HttpDownloaderThrows_PropagatesException()
        {
            // Arrange
            var subredditName = "programming";
            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromException<string>(new HttpRequestException("Network error")));

            // Act
            var test = await Should.ThrowAsync<HttpRequestException>(()
                => _extractor.GetSubredditImageUrlAsync(subredditName));

            // Assert
            test.Message.ShouldBe("Network error");
        }

        private static string CreateJsonWithImageKey(string key, string value)
        {
            return JsonSerializer.Serialize(new
            {
                data = new Dictionary<string, object>
                {
                    { key, value }
                }
            });
        }
    }
}
