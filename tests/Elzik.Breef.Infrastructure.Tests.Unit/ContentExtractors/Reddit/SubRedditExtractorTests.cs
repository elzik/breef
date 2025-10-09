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
        private const string DefaultRedditFallbackImageUrl = "https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg";
        
        private readonly IHttpDownloader _mockHttpDownloader;
        private readonly IOptions<RedditOptions> _mockRedditOptions;
        private readonly SubRedditContentExtractor _extractor;

        public SubRedditExtractorTests()
        {
            _mockHttpDownloader = Substitute.For<IHttpDownloader>();
            // Set up different responses for different URLs
            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith("new.json")))
                .Returns(Task.FromResult("<html><body>Mocked content</body></html>"));
            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith("about.json")))
                .Returns(Task.FromResult(JsonSerializer.Serialize(new { data = new { } })));
            
            _mockRedditOptions = Substitute.For<IOptions<RedditOptions>>();
            _mockRedditOptions.Value.Returns(new RedditOptions
            {
                DefaultBaseAddress = "https://www.reddit.com",
                AdditionalBaseAddresses = ["https://reddit.com"]
            });
            
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
            result.PreviewImageUrl.ShouldBe(DefaultRedditFallbackImageUrl);
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
            result.PreviewImageUrl.ShouldBe(DefaultRedditFallbackImageUrl);
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
            result.ShouldBe(DefaultRedditFallbackImageUrl);
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
            result.ShouldBe(DefaultRedditFallbackImageUrl);
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

        [Theory]
        [InlineData("icon_img", null)]
        [InlineData("community_icon", "")]
        [InlineData("banner_background_image", "   ")]
        [InlineData("banner_img", "\t")]
        [InlineData("mobile_banner_image", "\n")]
        public async Task GetSubredditImageUrlAsync_ImageUrlIsUnsuitable_UsesDefaultImageUrl(string imageKey, string? imageUrl)
        {
            // Arrange
            var subredditName = "programming";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(DefaultRedditFallbackImageUrl);
        }

        [Theory]
        [InlineData("icon_img", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==")]
        [InlineData("community_icon", "ftp://example.com/image.png")]
        [InlineData("banner_background_image", "file:///c:/images/banner.png")]
        [InlineData("banner_img", "javascript:alert('xss')")]
        [InlineData("mobile_banner_image", "mailto:test@example.com")]
        public async Task GetSubredditImageUrlAsync_ImageUrlHasNonHttpScheme_UsesDefaultImageUrl(string imageKey, string imageUrl)
        {
            // Arrange
            var subredditName = "programming";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(DefaultRedditFallbackImageUrl);
        }

        [Theory]
        [InlineData("icon_img", "not-a-valid-url")]
        [InlineData("community_icon", "://invalid-url")]
        [InlineData("banner_background_image", "http://")]
        [InlineData("banner_img", "https://")]
        public async Task GetSubredditImageUrlAsync_ImageUrlIsInvalidUri_UsesDefaultImageUrl(string imageKey, string imageUrl)
        {
            // Arrange
            var subredditName = "programming";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(DefaultRedditFallbackImageUrl);
        }

        [Fact]
        public async Task GetSubredditImageUrlAsync_MixedValidAndInvalidUrls_UsesFirstValidHttpUrl()
        {
            // Arrange
            var subredditName = "programming";
            var validImageUrl = "https://img.reddit.com/valid-icon.png";
            
            var json = JsonSerializer.Serialize(new
            {
                data = new Dictionary<string, object>
                {
                    { "banner_background_image", "data:image/png;base64,invalid" }, // Invalid scheme - should be skipped
                    { "banner_img", "" }, // Empty - should be skipped
                    { "mobile_banner_image", "   " }, // Whitespace - should be skipped
                    { "icon_img", validImageUrl }, // Valid HTTP URL - should be used
                    { "community_icon", "https://img.reddit.com/another-icon.png" } // Valid but comes after
                }
            });

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));
            _mockHttpDownloader.TryGet(validImageUrl).Returns(true);

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(validImageUrl);
        }

        [Theory]
        [InlineData("null")]
        [InlineData("empty")]
        [InlineData("whitespace")]
        [InlineData("non-http")]
        [InlineData("invalid-uri")]
        public async Task ExtractAsync_ImageUrlIsInvalid_UsesDefaultImageUrl(string invalidType)
        {
            // Arrange
            var url = "https://www.reddit.com/r/subreddit";
            string? imageUrl = invalidType switch
            {
                "null" => null,
                "empty" => "",
                "whitespace" => "   ",
                "non-http" => "data:image/png;base64,invalid",
                "invalid-uri" => "not-a-valid-url",
                _ => throw new ArgumentException($"Unknown invalid type: {invalidType}")
            };
            
            var json = CreateJsonWithImageKey("icon_img", imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith("new.json")))
                .Returns(Task.FromResult("<html><body>Mocked content</body></html>"));
            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith("about.json")))
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(DefaultRedditFallbackImageUrl);
        }

        private static string CreateJsonWithImageKey(string key, string? value)
        {
            var data = new Dictionary<string, object?>();
            if (value != null)
            {
                data[key] = value;
            }
            else
            {
                data[key] = null;
            }

            return JsonSerializer.Serialize(new { data });
        }
    }
}
