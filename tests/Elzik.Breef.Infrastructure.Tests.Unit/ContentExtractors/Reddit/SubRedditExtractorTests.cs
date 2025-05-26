using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using NSubstitute;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit
{
    public class SubRedditExtractorTests
    {
        private readonly IHttpDownloader _mockHttpDownloader;
        private readonly SubRedditContentExtractor _extractor;

        public SubRedditExtractorTests()
        {
            _mockHttpDownloader = Substitute.For<IHttpDownloader>();
            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult("<html><body>Mocked content</body></html>"));
            _extractor = new SubRedditContentExtractor(_mockHttpDownloader);
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
            Assert.Equal(imageUrl, result.PreviewImageUrl);
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
            Assert.Equal("https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg", result.PreviewImageUrl);
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
            Assert.Equal("https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg", result.PreviewImageUrl);
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
            Assert.Equal($"New in r/subreddit", result.Title);
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
            var extractor = new SubRedditContentExtractor(_mockHttpDownloader);
            var result = await extractor.ExtractAsync(url);

            // Assert
            Assert.Equal(json, result.Content);
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
