using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors
{
    public class HtmlContentExtractorTests
    {
        [Theory]
        [InlineData("TestHtmlPage.html", "TestHtmlPage-ExpectedContent.txt", "Test HTML Page", "https://test-large-image.jpg")]
        [InlineData("TestHtmlPageNoTitle.html", "TestHtmlPage-ExpectedContent.txt", "https://mock.url", "https://test-large-image.jpg")]
        [InlineData("BbcNewsPage.html", "BbcNewsPage-ExpectedContent.txt", "Artificial Intelligence: Plan to 'unleash AI' across UK revealed", "https://ichef.bbci.co.uk/ace/standard/1280/cpsprodpb/39ee/live/a2827620-d181-11ef-87df-d575b9a434a4.jpg")]
        [InlineData("TestHtmlPageNoContent.html", "TestHtmlPageNoContent-ExpectedContent.txt", "Test HTML Page", "https://test-large-image.jpg")]
        [InlineData("TestHtmlPageNoImages.html", "TestHtmlPage-ExpectedContent.txt", "Test HTML Page", null)]

        public async Task Extract_WithValidUrl_ExtractsContent(string testFileName, string expectedFileName, string expectedTitle, string? expectedPreviewImageUrl)
        {
            // Arrange
            var mockTestUrl = "https://mock.url";
            var mockHttpDownloader = Substitute.For<IHttpDownloader>();
            var testHtml = await File.ReadAllTextAsync(Path.Join("../../../../TestData", testFileName));
            mockHttpDownloader.DownloadAsync(Arg.Is(mockTestUrl)).Returns(Task.FromResult(testHtml));

            // Act
            var extractor = new HtmlContentExtractor(mockHttpDownloader);
            var result = await extractor.ExtractAsync(mockTestUrl);

            // Assert
            var expected = await File.ReadAllTextAsync(Path.Join("../../../../TestData", expectedFileName));

            var lineEndingNormalisedExpected = NormaliseLineEndings(expected);
            var lineEndingNormalisedContent = NormaliseLineEndings(result.Content);

            lineEndingNormalisedContent.ShouldBe(lineEndingNormalisedExpected);
            result.Title.ShouldBe(expectedTitle);
            result.PreviewImageUrl.ShouldBe(expectedPreviewImageUrl);
        }

        [Fact]
        public void CanHandle_AnyString_CanHandle()
        {
            // Arrange
            var mockHttpDownloader = Substitute.For<IHttpDownloader>();

            // Act
            var defaultOnlyContentExtractorStrategy = new HtmlContentExtractor(mockHttpDownloader);
            var canHandleAnyString = defaultOnlyContentExtractorStrategy.CanHandle("Any string.");

            // Assert
            canHandleAnyString.ShouldBeTrue();
        }

        private static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}