using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration
{
    public class ContentExtractorTests
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
            var mockHttpClient = Substitute.For<IWebPageDownloader>();
            var testHtml = await File.ReadAllTextAsync(Path.Join("../../../../TestData", testFileName));
            mockHttpClient.DownloadAsync(Arg.Is(mockTestUrl)).Returns(Task.FromResult(testHtml));

            // Act
            var extractor = new ContentExtractor(mockHttpClient);
            var result = await extractor.ExtractAsync(mockTestUrl);

            // Assert
            var expected = await File.ReadAllTextAsync(Path.Join("../../../../TestData", expectedFileName));

            var lineEndingNormalisedExpected = NormaliseLineEndings(expected);
            var lineEndingNormalisedContent = NormaliseLineEndings(result.Content);

            lineEndingNormalisedContent.ShouldBe(lineEndingNormalisedExpected);
            result.Title.ShouldBe(expectedTitle);
            result.PreviewImageUrl.ShouldBe(expectedPreviewImageUrl);
        }

        private static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}