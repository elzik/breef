using Elzik.Breef.Infrastructure;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Domain.Tests.Integration
{
    public class ContentExtractorTests
    {
        [Theory]
        [InlineData("TestHtmlPage.html", "TestHtmlPage-ExpectedContent.txt", "Test HTML Page")]
        [InlineData("TestHtmlPageNoTitle.html", "TestHtmlPage-ExpectedContent.txt", "https://mock.url")]
        [InlineData("BbcNewsPage.html", "BbcNewsPage-ExpectedContent.txt", "Artificial Intelligence: Plan to 'unleash AI' across UK revealed")]
        [InlineData("TestHtmlPageNoContent.html", "TestHtmlPageNoContent-ExpectedContent.txt", "Test HTML Page")]
        public async Task Extract_WithValidUrl_ExtractsContent(string testFileName, string expectedFileName, string expectedTitle)
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
        }

        private static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}