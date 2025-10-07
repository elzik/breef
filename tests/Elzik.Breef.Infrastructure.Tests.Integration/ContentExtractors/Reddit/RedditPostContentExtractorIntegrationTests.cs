using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using Microsoft.Extensions.Options;
using Refit;
using Shouldly;
using System.Text.Json;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors.Reddit
{
    public class RedditPostContentExtractorTests
    {
        private static bool IsRunningInGitHubWorkflow => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

        private readonly RedditPostContentExtractor _extractor;

        public RedditPostContentExtractorTests(ITestOutputHelper testOutputHelper)
        {
            var rawRedditClient = RestService.For<IRawRedditPostClient>("https://www.reddit.com/");
            var transformer = new RawRedditPostTransformer();
            var redditPostClient = new RedditPostClient(rawRedditClient, transformer);
            var logger = new TestOutputFakeLogger<HttpDownloader>(testOutputHelper);
            var httpDownloaderOptions = Options.Create(new HttpDownloaderOptions());
            var httpDownloader = new HttpDownloader(logger, httpDownloaderOptions);
            var redditOptions = Options.Create(new RedditOptions());
            var subredditImageExtractor = new SubRedditContentExtractor(httpDownloader, redditOptions);
            
            _extractor = new RedditPostContentExtractor(redditPostClient, subredditImageExtractor, redditOptions);
        }

        [SkippableTheory]
        [InlineData("https://www.reddit.com/r/learnprogramming/comments/1kqiwzc")]
        [InlineData("https://reddit.com/r/learnprogramming/comments/1kqiwzc/")]
        [InlineData("https://www.reddit.com/r/learnprogramming/comments/1kqiwzc/title")]
        public async Task ExtractAsync_RealRedditPost_ReturnsValidExtract(string url)
        {
            // Arrange
            SkipIfInGitHubWorkflow();

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldNotBeNullOrWhiteSpace();
            result.Content.ShouldNotBeNullOrWhiteSpace();
            result.PreviewImageUrl.ShouldNotBeNullOrWhiteSpace();

            var redditPost = JsonSerializer.Deserialize<RedditPost>(result.Content);
            redditPost.ShouldNotBeNull();
            redditPost.Post.ShouldNotBeNull();
            redditPost.Post.Id.ShouldBe("1kqiwzc");
            redditPost.Post.Title.ShouldNotBeNullOrWhiteSpace();
            redditPost.Comments.ShouldNotBeNull();
        }

        [SkippableFact]
        public async Task ExtractAsync_PostWithImage_UsesPostImage()
        {
            // Arrange
            SkipIfInGitHubWorkflow();

            var urlWithKnownGoodImage = "https://www.reddit.com/r/BBQ/comments/1nxust6/have_anyone_use_coconut_shell_as_smoke";

            // Act
            var result = await _extractor.ExtractAsync(urlWithKnownGoodImage);

            // Assert
            result.ShouldNotBeNull();
            result.PreviewImageUrl.ShouldNotBeNull();
            result.PreviewImageUrl.ShouldBe("https://preview.redd.it/olmpl5vmp3tf1.jpeg?auto=webp&s=1cb106a6fab1ddd48bcf8e9afdd2a06ca22d46ba");
        }

        [SkippableFact]
        public async Task ExtractAsync_PostWithoutImage_UsesSubredditFallback()
        {
            // Arrange
            SkipIfInGitHubWorkflow();

            var urlWithNoImage = "https://www.reddit.com/r/bristol/comments/1nzoyrd/parking_near_cotham_school";

            // Act
            var result = await _extractor.ExtractAsync(urlWithNoImage);

            // Assert
            result.ShouldNotBeNull();
            result.PreviewImageUrl.ShouldNotBeNull();
            result.PreviewImageUrl.ShouldBe("https://b.thumbs.redditmedia.com/fMCtUDLMEEt1SrDtRyg1v1xiXVoXmP_3dxScj1kgzoE.png");
        }

        [SkippableFact]
        public async Task ExtractAsync_PostAndSubredditWithoutImage_UsesRedditFallback()
        {
            // Arrange
            SkipIfInGitHubWorkflow();

            var urlWithNoImage = "https://www.reddit.com/r/PleX/comments/1nsxi8p/the_recent_data_breach_looks_to_have_been_made";

            // Act
            var result = await _extractor.ExtractAsync(urlWithNoImage);

            // Assert
            result.ShouldNotBeNull();
            result.PreviewImageUrl.ShouldNotBeNull();
            result.PreviewImageUrl.ShouldBe("https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg");
        }

        [SkippableFact]
        public async Task ExtractAsync_ValidPost_ContentContainsCompleteRedditStructure()
        {
            // Arrange
            SkipIfInGitHubWorkflow();

            var url = "https://www.reddit.com/r/learnprogramming/comments/1kqiwzc";

            // Act
            var result = await _extractor.ExtractAsync(url);

            // Assert
            var redditPost = JsonSerializer.Deserialize<RedditPost>(result.Content);
            redditPost.ShouldNotBeNull();

            // Verify post structure
            redditPost.Post.Id.ShouldNotBeNullOrEmpty();
            redditPost.Post.Title.ShouldNotBeNullOrEmpty();
            redditPost.Post.Author.ShouldNotBeNullOrEmpty();
            redditPost.Post.Subreddit.ShouldNotBeNullOrEmpty();
            redditPost.Post.CreatedUtc.ShouldNotBe(default);

            // Verify comments structure
            redditPost.Comments.ShouldNotBeNull();
            if (redditPost.Comments.Any())
            {
                var firstComment = redditPost.Comments[0];
                firstComment.Id.ShouldNotBeNullOrEmpty();
                firstComment.CreatedUtc.ShouldNotBe(default);
            }
        }

        [SkippableTheory]
        [InlineData("not-a-url")]
        [InlineData("https://reddit.com")]
        [InlineData("https://reddit.com/r/programming")]
        [InlineData("https://reddit.com/r/programming/posts/abc123/title")]
        [InlineData("https://not-reddit.com/r/programming/comments/abc123/title")]
        public async Task ExtractAsync_InvalidUrls_ThrowsInvalidOperationException(string invalidUrl)
        {
            // Arrange
            SkipIfInGitHubWorkflow();

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(() => _extractor.ExtractAsync(invalidUrl));
        }

        [SkippableFact]
        public async Task ExtractAsync_NonExistentPost_ThrowsExpectedException()
        {
            // Arrange
            SkipIfInGitHubWorkflow();

            var url = "https://www.reddit.com/r/programming/comments/nonexistent123/title";

            // Act
            var ex = await Should.ThrowAsync<ApiException>(() => _extractor.ExtractAsync(url));

            // Assert
            ex.Message.ShouldBe("Response status code does not indicate success: 404 (Not Found).");
        }

        [Theory]
        [InlineData("https://reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://reddit.com/r/programming/comments/abc123")]
        [InlineData("https://www.reddit.com/r/funny/comments/def456/joke")]
        [InlineData("https://www.reddit.com/r/funny/comments/def456")]
        [InlineData("https://REDDIT.COM/r/AskReddit/comments/ghi789/question")]
        [InlineData("https://REDDIT.COM/r/AskReddit/comments/ghi789")]
        [InlineData("https://reddit.com/r/pics/comments/jkl012/image/")]
        [InlineData("https://reddit.com/r/pics/comments/jkl012/")]
        public void CanHandle_VariousValidUrls_ReturnsTrue(string validUrl)
        {
            // Act
            var canHandle = _extractor.CanHandle(validUrl);

            // Assert
            canHandle.ShouldBeTrue($"Should handle URL: {validUrl}");
        }

        [Theory]
        [InlineData("https://reddit.com/r/programming")]
        [InlineData("https://reddit.com/r/programming/hot")]
        [InlineData("https://reddit.com/r/programming/comments")]
        [InlineData("https://reddit.com/r/programming/comments/abc123/title/extra")]
        [InlineData("https://reddit.com/user/username/comments/abc123/title")]
        [InlineData("https://old.reddit.com/r/programming/comments/abc123/title")]
        [InlineData("https://youtube.com/r/programming/comments/abc123/title")]
        public void CanHandle_VariousInvalidUrls_ReturnsFalse(string invalidUrl)
        {
            // Act
            var canHandle = _extractor.CanHandle(invalidUrl);

            // Assert
            canHandle.ShouldBeFalse($"Should not handle URL: {invalidUrl}");
        }

        private static void SkipIfInGitHubWorkflow(string reason = "Skipped because requests to reddit.com from GitHub workflows " +
            "are always blocked meaning this test case always fails. This must be run locally instead.")
        {
            Skip.If(IsRunningInGitHubWorkflow, reason);
        }
    }
}