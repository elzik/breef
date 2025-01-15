using FluentAssertions;

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
            var lineEndingNormalisedResult = result.Replace("\r\n", "\n");

            // Assert
            var expectedSource = await File.ReadAllTextAsync("../../../../TestData/TestHtmlPage.html");
            lineEndingNormalisedResult.Should().Be(expectedSource);
        }
    }
}