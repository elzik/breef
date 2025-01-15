using Elzik.Breef.Infrastructure;
using FluentAssertions;
using NSubstitute;

namespace Elzik.Breef.Domain.Tests.Integration
{
    public class ContentExtractorTests
    {
        [Theory]
        [InlineData("TestHtmlPage.html", "TestHtmlPage-ExpectedContent.txt")]
        [InlineData("BbcNewsPage.html", "BbcNewsPage-ExpectedContent.txt")]
        public async Task Extract_WithValidUrl_ExtractsContent(string testFileName, string expectedFileName)
        {
            // Arrange
            var mockTestUrl = "https://mock.url";
            var mockHttpClient = Substitute.For<IWebPageDownloader>();
            var testHtml = await File.ReadAllTextAsync(Path.Join("../../../../TestData", testFileName));
            mockHttpClient.DownloadAsync(Arg.Is(mockTestUrl)).Returns(Task.FromResult(testHtml));

            // Act
            var extractor = new ContentExtractor(mockHttpClient);
            var result = await extractor.Extract(mockTestUrl);

            // Assert
            var expected = await File.ReadAllTextAsync(Path.Join("../../../../TestData", expectedFileName));
            var lineEndingNormalisedResult = result.Replace("\r\n", "\n");

            lineEndingNormalisedResult.Should().Be(expected);
        }
    }
}