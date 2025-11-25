using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit
{
    public class RedditPostContentExtractorTests
    {
        private readonly IRedditPostClient _mockRedditPostClient;
        private readonly ISubredditImageExtractor _mockSubredditImageExtractor;
        private readonly IOptions<RedditOptions> _mockRedditOptions;
        private readonly RedditPostContentExtractor _extractor;

        public RedditPostContentExtractorTests()
        {
            _mockRedditPostClient = Substitute.For<IRedditPostClient>();
            _mockSubredditImageExtractor = Substitute.For<ISubredditImageExtractor>();
            _mockRedditOptions = Substitute.For<IOptions<RedditOptions>>();
            _mockRedditOptions.Value.Returns(new RedditOptions());
            
            _extractor = new RedditPostContentExtractor(_mockRedditPostClient, _mockSubredditImageExtractor, _mockRedditOptions);
        }

        [Theory]
        [InlineData("https://reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://reddit.com/r/programming/comments/abc123/title/")]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123/title/")]
        [InlineData("https://reddit.com/r/programming/comments/abc123")]
        [InlineData("https://reddit.com/r/programming/comments/abc123/")]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123")]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123/")]
        [InlineData("hTTpS://rEDDiT.cOm/R/pRoGrAmMiNg/CoMmEnTs/AbC123/TiTlE")]
        [InlineData("hTTpS://rEDDiT.cOm/R/pRoGrAmMiNg/CoMmEnTs/AbC123")]
        public void CanHandle_ValidRedditPostUrl_ReturnsTrue(string url)
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
        [InlineData("https://reddit.com/r/programming")]
        [InlineData("https://reddit.com/r/programming/comments")]
        [InlineData("https://reddit.com/r/programming/comments/abc123/title/extra")]
        [InlineData("https://not-reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://www2.reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://reddit.com/r/programming/posts/abc123/title")]
        [InlineData("https://reddit.com/user/username/comments/abc123/title")]
        [InlineData("https://reddit.com/r/programming/comments/abc123/title/extra/segment")]
        public void CanHandle_InvalidRedditPostUrl_ReturnsFalse(string url)
        {
            // Act
            var canHandle = _extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeFalse();
        }

        [Theory]
        [InlineData("https://custom.reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://alt.reddit.instance.com/r/programming/comments/abc123/title")]
        public void CanHandle_CustomRedditInstance_ReturnsTrue(string url)
        {
            // Arrange
            var customOptions = new RedditOptions
            {
                DefaultBaseAddress = "https://www.reddit.com",
                AdditionalBaseAddresses = ["https://reddit.com", "https://custom.reddit.com", "https://alt.reddit.instance.com"]
            };
            _mockRedditOptions.Value.Returns(customOptions);
            var extractor = new RedditPostContentExtractor(_mockRedditPostClient, _mockSubredditImageExtractor, _mockRedditOptions);

            // Act
            var canHandle = extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeTrue();
        }

        [Theory]
        [InlineData("https://unknown.reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://www.unknown.reddit.com/r/programming/comments/abc123/title")]
        public void CanHandle_UnknownRedditInstance_ReturnsFalse(string url)
        {
            // Arrange
            var customOptions = new RedditOptions
            {
                DefaultBaseAddress = "https://www.reddit.com",
                AdditionalBaseAddresses = ["https://reddit.com", "https://custom.reddit.com"]
            };
            _mockRedditOptions.Value.Returns(customOptions);
            var extractor = new RedditPostContentExtractor(_mockRedditPostClient, _mockSubredditImageExtractor, _mockRedditOptions);

            // Act
            var canHandle = extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeFalse();
        }

        [Theory]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123")]
        public async Task ExtractAsync_ValidUrl_CallsRedditPostClientWithCorrectPostId(string url)
        {
            // Arrange
            var testPost = CreateTestRedditPost("abc123", "Test Title", "https://example.com/image.jpg");
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);

            // Act
            await _extractor.ExtractAsync(url);

            // Assert
            await _mockRedditPostClient.Received(1).GetPost("abc123");
        }

        [Fact]
        public async Task ExtractAsync_PostWithImage_ReturnsExtractWithPostImage()
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var postImageUrl = "https://i.redd.it/post-image.jpg";
            var testPost = CreateTestRedditPost("abc123", "Test Title", postImageUrl);
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(postImageUrl);
            await _mockSubredditImageExtractor.DidNotReceive().GetSubredditImageUrlAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ExtractAsync_PostWithoutImage_UsesSubredditFallbackImage()
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var subredditImageUrl = "https://styles.redditmedia.com/programming-icon.png";
            var testPost = CreateTestRedditPost("abc123", "Test Title", null);
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);
            _mockSubredditImageExtractor.GetSubredditImageUrlAsync("programming").Returns(subredditImageUrl);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(subredditImageUrl);
            await _mockSubredditImageExtractor.Received(1).GetSubredditImageUrlAsync("programming");
        }

        [Fact]
        public async Task ExtractAsync_PostWithEmptyImage_UsesSubredditFallbackImage()
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var subredditImageUrl = "https://styles.redditmedia.com/programming-icon.png";
            var testPost = CreateTestRedditPost("abc123", "Test Title", "");
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);
            _mockSubredditImageExtractor.GetSubredditImageUrlAsync("programming").Returns(subredditImageUrl);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(subredditImageUrl);
            await _mockSubredditImageExtractor.Received(1).GetSubredditImageUrlAsync("programming");
        }

        [Fact]
        public async Task ExtractAsync_PostWithWhitespaceImage_UsesSubredditFallbackImage()
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var subredditImageUrl = "https://styles.redditmedia.com/programming-icon.png";
            var testPost = CreateTestRedditPost("abc123", "Test Title", "   ");
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);
            _mockSubredditImageExtractor.GetSubredditImageUrlAsync("programming").Returns(subredditImageUrl);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(subredditImageUrl);
            await _mockSubredditImageExtractor.Received(1).GetSubredditImageUrlAsync("programming");
        }

        [Fact]
        public async Task ExtractAsync_ValidUrl_ReturnsCorrectTitle()
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var expectedTitle = "How to write better code";
            var testPost = CreateTestRedditPost("abc123", expectedTitle, "https://example.com/image.jpg");
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.Title.ShouldBe(expectedTitle);
        }

        [Fact]
        public async Task ExtractAsync_ValidUrl_ReturnsSerializedPostAsContent()
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var testPost = CreateTestRedditPost("abc123", "Test Title", "https://example.com/image.jpg", url);
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            var deserializedPost = JsonSerializer.Deserialize<RedditPost>(result.Content);
            deserializedPost.ShouldNotBeNull();
            deserializedPost.Post.Id.ShouldBe("abc123");
            deserializedPost.Post.Title.ShouldBe("Test Title");
            deserializedPost.Post.PostUrl.ShouldBe(url);
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExtractAsync_InvalidUrl_ThrowsInvalidOperationException(string invalidUrl)
        {
            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(() => _extractor.ExtractAsync(invalidUrl));
        }

        [Theory]
        [InlineData("https://reddit.com")]
        [InlineData("https://reddit.com/r/programming")]
        [InlineData("https://reddit.com/r/programming/comments")]
        [InlineData("https://reddit.com/r/programming/posts/abc123/title")]
        [InlineData("https://reddit.com/r/programming/comments/abc123/title/extra")]
        public async Task ExtractAsync_UnsupportedUrl_ThrowsInvalidOperationException(string unsupportedUrl)
        {
            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(() => _extractor.ExtractAsync(unsupportedUrl));
        }

        [Fact]
        public async Task ExtractAsync_InvalidUrl_ThrowsWithMeaningfulErrorMessage()
        {
            // Arrange
            var invalidUrl = "not-a-valid-url";

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => _extractor.ExtractAsync(invalidUrl));
            exception.Message.ShouldContain("Invalid URL format");
            exception.Message.ShouldContain(invalidUrl);
            exception.Message.ShouldContain("valid absolute URI");
        }

        [Fact]
        public async Task ExtractAsync_UnsupportedUrl_ThrowsWithMeaningfulErrorMessage()
        {
            // Arrange
            var unsupportedUrl = "https://reddit.com/r/programming";

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => _extractor.ExtractAsync(unsupportedUrl));
            exception.Message.ShouldContain("Unsupported Reddit URL format");
            exception.Message.ShouldContain(unsupportedUrl);
            exception.Message.ShouldContain("Expected format");
            exception.Message.ShouldContain("reddit-domain");
        }

        [Fact]
        public async Task ExtractAsync_UnsupportedHost_ThrowsWithMeaningfulErrorMessage()
        {
            // Arrange
            var unsupportedHostUrl = "https://not-reddit.com/r/programming/comments/abc123/title";

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => _extractor.ExtractAsync(unsupportedHostUrl));
            exception.Message.ShouldContain("Unsupported domain");
            exception.Message.ShouldContain("not-reddit.com");
            exception.Message.ShouldContain("Supported domains");
        }

        [Theory]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123/title", "programming")]
        [InlineData("https://www.reddit.com/r/programming/comments/abc123", "programming")]
        [InlineData("https://www.reddit.com/r/funny/comments/def456/joke", "funny")]
        [InlineData("https://www.reddit.com/r/funny/comments/def456", "funny")]
        [InlineData("https://www.reddit.com/r/todayilearned/comments/ghi789/fact", "todayilearned")]
        [InlineData("https://www.reddit.com/r/todayilearned/comments/ghi789", "todayilearned")]
        [InlineData("https://www.reddit.com/r/AskReddit/comments/jkl012/question", "AskReddit")]
        [InlineData("https://www.reddit.com/r/AskReddit/comments/jkl012", "AskReddit")]
        public async Task ExtractAsync_DifferentSubreddits_CallsSubredditImageExtractorWithCorrectName(string url, string expectedSubreddit)
        {
            // Arrange
            var testPost = CreateTestRedditPost("test123", "Test Title", null);
            _mockRedditPostClient.GetPost(Arg.Any<string>()).Returns(testPost);
            _mockSubredditImageExtractor.GetSubredditImageUrlAsync(expectedSubreddit)
                .Returns($"https://styles.redditmedia.com/{expectedSubreddit}-icon.png");

            // Act
            await _extractor.ExtractAsync(url);

            // Assert
            await _mockSubredditImageExtractor.Received(1).GetSubredditImageUrlAsync(expectedSubreddit);
        }

        [Theory]
        [InlineData("https://i.redd.it/gallery-image.jpg")]
        [InlineData("https://preview.redd.it/preview-image.png")]
        [InlineData("https://external-preview.redd.it/external-image.gif")]
        [InlineData("https://imgur.com/direct-link.webp")]
        [InlineData("https://reddit.com/thumbnail.bmp")]
        public async Task ExtractAsync_PostWithVariousImageUrls_DoesNotUseFallback(string imageUrl)
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var testPost = CreateTestRedditPost("abc123", "Test Title", imageUrl);
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.PreviewImageUrl.ShouldBe(imageUrl);
            await _mockSubredditImageExtractor.DidNotReceive().GetSubredditImageUrlAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ExtractAsync_SubredditImageExtractorThrows_PropagatesException()
        {
            // Arrange
            var url = "https://www.reddit.com/r/programming/comments/abc123/title";
            var testPost = CreateTestRedditPost("abc123", "Test Title", null);
            _mockRedditPostClient.GetPost("abc123").Returns(testPost);
            _mockSubredditImageExtractor.GetSubredditImageUrlAsync("programming")
                .Returns(Task.FromException<string>(new HttpRequestException("Network error")));

            // Act & Assert
            await Should.ThrowAsync<HttpRequestException>(() => _extractor.ExtractAsync(url));
        }

        private static RedditPost CreateTestRedditPost(string id, string title, string? imageUrl, string? postUrl = null) => new()
        {
                Post = new RedditPostContent
                {
                    Id = id,
                    Title = title,
                    Author = "testuser",
                    Subreddit = "testsubreddit",
                    Score = 100,
                    Content = "Test post content",
                    CreatedUtc = DateTime.UtcNow,
                    ImageUrl = imageUrl,
                    PostUrl = postUrl ?? $"https://reddit.com/r/testsubreddit/comments/{id}"
                },
                Comments =
                [
                    new() {
                        Id = "comment1",
                        Author = "commenter1",
                        Score = 50,
                        Content = "Test comment",
                        CreatedUtc = DateTime.UtcNow,
                        Replies = []
                    }
                ]
            };
    }
}