using FluentAssertions;

namespace Elzik.Breef.Infrastructure.Tests.Integration
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStringAsync_WithUrlFromStaticPage_ReturnsString()
        {
            // Arrange
            var testUrl = "https://elzik.github.io/test-web/test.html";

            // Act
            var httpClient = new HttpClientWrapper();
            var result = await httpClient.GetStringAsync(testUrl);
            var lineEndingNormalisedResult = result.Replace("\r\n", "\n");

            // Assert
            var expectedSource = await File.ReadAllTextAsync("../../../../TestData/TestHtmlPage.html");
            lineEndingNormalisedResult.Should().Be(expectedSource);
        }
    }
}