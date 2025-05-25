using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors
{
    public class SubRedditExtractorTests
    {
        private readonly IHttpDownloader _mockHttpDownloader;

        public SubRedditExtractorTests()
        {
            _mockHttpDownloader = Substitute.For<IHttpDownloader>();
            _mockHttpDownloader.DownloadAsync(Arg.Any<string>())
                .Returns(Task.FromResult("<html><body>Mocked content</body></html>"));
        }

        [Theory]
        [InlineData("https://reddit.com/r/testsubreddit/")]
        [InlineData("https://reddit.com/r/testsubreddit")]
        [InlineData("hTTpS://rEDdiT.cOm/R/tEsTsUbReDdIt/")]
        [InlineData("https://www.reddit.com/r/testsubreddit/")]
        public void CanHandle_ValidSubRedditUrl_ReturnsTrue(string url)
        {
            // Arrange
            var extractor = new SubRedditContentExtractor(_mockHttpDownloader);

            // Act
            var canHandle = extractor.CanHandle(url);

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
            // Arrange
            var extractor = new SubRedditContentExtractor(_mockHttpDownloader);

            // Act
            var canHandle = extractor.CanHandle(url);

            // Assert
            canHandle.ShouldBeFalse();
        }
    }
}
