using Microsoft.Extensions.Options;
using Shouldly;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration
{
    public class WebPageDownloaderTests(ITestOutputHelper testOutputHelper)
    {

        private readonly IOptions<WebPageDownLoaderOptions> _defaultOptions = Options.Create(new WebPageDownLoaderOptions());
        private readonly TestOutputFakeLogger<WebPageDownloader> _testOutputFakeLogger = new(testOutputHelper);

        [Fact]
        public async Task DownloadAsync_WithUrlFromStaticPage_ReturnsString()
        {
            // Arrange
            var testUrl = "https://elzik.github.io/test-web/test.html";

            // Act
            var httpClient = new WebPageDownloader(_testOutputFakeLogger, _defaultOptions);
            var result = await httpClient.DownloadAsync(testUrl);

            // Assert
            var expectedSource = await File.ReadAllTextAsync("../../../../TestData/StaticTestPage.html");

            var lineEndingNormalisedExpected = NormaliseLineEndings(expectedSource);
            var lineEndingNormalisedResult = NormaliseLineEndings(result);

            lineEndingNormalisedResult.ShouldBe(lineEndingNormalisedExpected);
        }

        [Fact]
        public async Task DownloadAsync_WithUrlFromStaticPage_LogsUserAgent()
        {
            // Arrange
            var testUrl = "https://elzik.github.io/test-web/test.html";

            // Act
            var httpClient = new WebPageDownloader(_testOutputFakeLogger, _defaultOptions);
            await httpClient.DownloadAsync(testUrl);

            // Assert
            var logCollector = _testOutputFakeLogger.FakeLogger.Collector;
            logCollector.Count.ShouldBe(1);
            _testOutputFakeLogger.FakeLogger.Collector.LatestRecord.Level.ShouldBe(
                Microsoft.Extensions.Logging.LogLevel.Information);
            _testOutputFakeLogger.FakeLogger.Collector.LatestRecord.Message.ShouldBe(
                "Downloads will be made using the User-Agent: Mozilla/5.0, (Windows NT 10.0; Win64; x64), AppleWebKit/537.36, (KHTML, like Gecko), Chrome/110.0.0.0, Safari/537.36");

        }

        [Theory]
        [InlineData("https://reddit.com")]
        [InlineData("https://stackoverflow.com/")]
        public async Task DownloadAsync_ForBlockedSites_ThwartsBlock(string testUrl)
        {
            // Act
            var httpClient = new WebPageDownloader(_testOutputFakeLogger, _defaultOptions);
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