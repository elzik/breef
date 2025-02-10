using Microsoft.Extensions.Options;
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
            var defaultOptions = Options.Create(new WebPageDownLoaderOptions());

            // Act
            var httpClient = new WebPageDownloader(defaultOptions);
            var result = await httpClient.DownloadAsync(testUrl);

            // Assert
            var expectedSource = await File.ReadAllTextAsync("../../../../TestData/StaticTestPage.html");

            var lineEndingNormalisedExpected = NormaliseLineEndings(expectedSource);
            var lineEndingNormalisedResult = NormaliseLineEndings(result);

            lineEndingNormalisedResult.ShouldBe(lineEndingNormalisedExpected);
        }

        [Theory]
        [InlineData("https://reddit.com")]
        [InlineData("https://stackoverflow.com/")]
        public async Task DownloadAsync_ForBlockedSites_ThwartsBlock(string testUrl)
        {
            // Arrange
            var defaultOptions = Options.Create(new WebPageDownLoaderOptions());

            // Act
            var httpClient = new WebPageDownloader(defaultOptions);
            var result = await httpClient.DownloadAsync(testUrl);

            // Assert
            result.ShouldNotBeNull();
        }


        private static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}