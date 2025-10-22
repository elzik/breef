using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit
{
    public class SubredditExtractorTests
    {
        private const string FallbackImageUrl = "https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg";
        
        private readonly ISubredditClient _mockSubredditClient;
        private readonly IHttpDownloader _mockHttpDownloader;
        private readonly IOptions<RedditOptions> _mockRedditOptions;
        private readonly SubredditContentExtractor _extractor;

        public SubredditExtractorTests()
        {
            _mockSubredditClient = Substitute.For<ISubredditClient>();
            _mockSubredditClient.GetNewInSubreddit(Arg.Any<string>())
                .Returns(new NewInSubreddit { Posts = new List<RedditPost>() });
            
            _mockHttpDownloader = Substitute.For<IHttpDownloader>();
            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(JsonSerializer.Serialize(new { data = new { } })));
            
            _mockRedditOptions = Substitute.For<IOptions<RedditOptions>>();
            _mockRedditOptions.Value.Returns(new RedditOptions
            {
                DefaultBaseAddress = "https://www.reddit.com",
                AdditionalBaseAddresses = ["https://reddit.com"],
                FallbackImageUrl = FallbackImageUrl
            });
            
            _extractor = new SubredditContentExtractor(_mockSubredditClient, _mockHttpDownloader, _mockRedditOptions);
        }

        [Theory]
        [InlineData("https://reddit.com/r/testsubreddit/")]
        [InlineData("https://reddit.com/r/testsubreddit")]
        [InlineData("hTTpS://rEDdiT.cOm/R/tEsTsUbReDdIt/")]
        [InlineData("https://www.reddit.com/r/testsubreddit/")]
        public void CanHandle_ValidSubredditUrl_ReturnsTrue(string url)
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
        public void CanHandle_InvalidSubredditUrl_ReturnsFalse(string url)
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
                AdditionalBaseAddresses = ["https://reddit.com", "https://custom.reddit.com", "https://alt.reddit.instance.com"],
                FallbackImageUrl = FallbackImageUrl
            };
            _mockRedditOptions.Value.Returns(customOptions);
            var extractor = new SubredditContentExtractor(_mockSubredditClient, _mockHttpDownloader, _mockRedditOptions);

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
                AdditionalBaseAddresses = ["https://reddit.com", "https://custom.reddit.com"],
                FallbackImageUrl = FallbackImageUrl
            };
            _mockRedditOptions.Value.Returns(customOptions);
            var extractor = new SubredditContentExtractor(_mockSubredditClient, _mockHttpDownloader, _mockRedditOptions);

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
        public async Task ExtractAsync_TryGetReturnsFalse_UsesFallbackImageUrl(string imageKey)
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
            result.PreviewImageUrl.ShouldBe(FallbackImageUrl);
        }

        [Fact]
        public async Task ExtractAsync_NoImageKeysExist_UsesFallbackImageUrl()
        {
            // Arrange
            var url = $"https://www.reddit.com/r/subreddit";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith(".json")))
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(FallbackImageUrl);
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
            var samplePost = new RedditPost
            {
                Post = new RedditPostContent
                {
                    Id = "abc123",
                    Title = "Test Post",
                    Author = "testuser",
                    Subreddit = "subreddit",
                    Score = 100,
                    Content = "Test content",
                    CreatedUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                Comments = new List<RedditComment>()
            };
            
            var newInSubreddit = new NewInSubreddit 
            { 
                Posts = new List<RedditPost> { samplePost } 
            };
            var expectedJson = JsonSerializer.Serialize(newInSubreddit);
            
            _mockSubredditClient.GetNewInSubreddit("subreddit").Returns(newInSubreddit);
            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith("about.json")))
                .Returns(Task.FromResult(JsonSerializer.Serialize(new { data = new { } })));

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.Content.ShouldBe(expectedJson);
            
            var deserializedContent = JsonSerializer.Deserialize<NewInSubreddit>(result.Content);
            deserializedContent.ShouldNotBeNull();
            deserializedContent.Posts.Count.ShouldBe(1);
            deserializedContent.Posts[0].Post.Id.ShouldBe("abc123");
            deserializedContent.Posts[0].Post.Title.ShouldBe("Test Post");
        }

        [Theory]
        [InlineData("https://www.reddit.com/r/testsubreddit")]
        [InlineData("https://www.reddit.com/r/testsubreddit/")]
        public async Task ExtractAsync_ValidUrl_CallsSubredditClientWithCorrectName(string subredditUrl)
        {
            // Arrange
            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith("about.json")))
                .Returns(Task.FromResult(JsonSerializer.Serialize(new { data = new { } })));

            // Act
            await _extractor.ExtractAsync(subredditUrl);

            // Assert
            await _mockSubredditClient.Received(1).GetNewInSubreddit("testsubreddit");
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
        public async Task GetSubredditImageUrlAsync_NoImageKeysExist_ReturnsFallbackImageUrl()
        {
            // Arrange
            var subredditName = "programming";
            var json = JsonSerializer.Serialize(new { data = new { } });

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(FallbackImageUrl);
        }

        [Fact]
        public async Task GetSubredditImageUrlAsync_ImageExistsButNotAccessible_ReturnsFallbackImageUrl()
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
            result.ShouldBe(FallbackImageUrl);
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
            result.ShouldBe(bannerImageUrl);
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
        [InlineData("icon_img", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==")]
        [InlineData("community_icon", "ftp://example.com/image.png")]
        [InlineData("banner_background_image", "file:///c:/images/banner.png")]
        [InlineData("banner_img", "javascript:alert('xss')")]
        [InlineData("mobile_banner_image", "mailto:test@example.com")]
        public async Task GetSubredditImageUrlAsync_ImageUrlIsUnsuitable_UsesFallbackImageUrl(string imageKey, string? imageUrl)
        {
            // Arrange
            var subredditName = "programming";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(FallbackImageUrl);
        }

        [Theory]
        [InlineData("icon_img", "not-a-valid-url")]
        [InlineData("community_icon", "://invalid-url")]
        [InlineData("banner_background_image", "http://")]
        [InlineData("banner_img", "https://")]
        public async Task GetSubredditImageUrlAsync_ImageUrlIsInvalidUri_UsesFallbackImageUrl(string imageKey, string imageUrl)
        {
            // Arrange
            var subredditName = "programming";
            var json = CreateJsonWithImageKey(imageKey, imageUrl);

            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.GetSubredditImageUrlAsync(subredditName);

            // Assert
            result.ShouldBe(FallbackImageUrl);
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
                    { "banner_background_image", "data:image/png;base64,invalid" },
                    { "banner_img", "" },
                    { "mobile_banner_image", "   " },
                    { "icon_img", validImageUrl },
                    { "community_icon", "https://img.reddit.com/another-icon.png" }
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
        public async Task ExtractAsync_ImageUrlIsInvalid_UsesFallbackImageUrl(string invalidType)
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

            _mockSubredditClient.GetNewInSubreddit("subreddit")
                .Returns(new NewInSubreddit { Posts = new List<RedditPost>() });
            _mockHttpDownloader.DownloadAsync(Arg.Is<string>(s => s.EndsWith("about.json")))
                .Returns(Task.FromResult(json));

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(FallbackImageUrl);
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
