using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration
{
    public class WebPageDownloaderTests
    {
        [Fact]
        public async Task DownloadAsync_WithUrlFromStaticPage_ReturnsString()
        {
            // Arrange
            var testUrl = "https://elzik.github.io/test-web/test.html";

            // Act
            var httpClient = new WebPageDownloader();
            var result = await httpClient.DownloadAsync(testUrl);

            // Assert
            var expectedSource = await File.ReadAllTextAsync("../../../../TestData/TestHtmlPage.html");

            var lineEndingNormalisedExpected = NormaliseLineEndings(expectedSource);
            var lineEndingNormalisedResult = NormaliseLineEndings(result);

            lineEndingNormalisedResult.ShouldBe(lineEndingNormalisedExpected);
        }

        private string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}