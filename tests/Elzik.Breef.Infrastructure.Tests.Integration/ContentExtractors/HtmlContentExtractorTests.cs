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
            var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
            var mockHttpClient = Substitute.For<HttpClient>();
            mockHttpClientFactory.CreateClient("BreefDownloader").Returns(mockHttpClient);

            var testHtml = await File.ReadAllTextAsync(Path.Join("../../../../TestData", testFileName));

            var mockHandler = new MockHttpMessageHandler(testHtml);
            var httpClient = new HttpClient(mockHandler);
            mockHttpClientFactory.CreateClient("BreefDownloader").Returns(httpClient);

            // Act
            var extractor = new HtmlContentExtractor(mockHttpClientFactory);
            var result = await extractor.ExtractAsync(mockTestUrl);

            // Assert
            var expected = await File.ReadAllTextAsync(Path.Join("../../../../TestData", expectedFileName));

            var lineEndingNormalisedExpected = NormaliseLineEndings(expected);
            var lineEndingNormalisedContent = NormaliseLineEndings(result.Content);

            lineEndingNormalisedContent.ShouldBe(lineEndingNormalisedExpected);
            result.Title.ShouldBe(expectedTitle);
            result.PreviewImageUrl.ShouldBe(expectedPreviewImageUrl);
            result.ExtractType.ShouldBe("HtmlContent");
        }

        [Fact]
        public void CanHandle_AnyString_CanHandle()
        {
            // Arrange
            var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();

            // Act
            var defaultOnlyContentExtractorStrategy = new HtmlContentExtractor(mockHttpClientFactory);
            var canHandleAnyString = defaultOnlyContentExtractorStrategy.CanHandle("Any string.");

            // Assert
            canHandleAnyString.ShouldBeTrue();
        }

        private static string NormaliseLineEndings(string text)
        {
            return text.Replace("\r\n", "\n");
        }

        private class MockHttpMessageHandler(string content) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(content)
                });
            }
        }
    }
}