using HtmlAgilityPack;

namespace Elzik.Breef.Domain
{
    public class ContentExtractor(IWebPageDownloader httpClient) : IContentExtractor
    {
        public async Task<string> Extract(string webPageUrl)
        {
            var html = await httpClient.DownloadAsync(webPageUrl);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var mainContentNode = htmlDocument.DocumentNode
                .SelectNodes("//div|//article|//p")
                ?.OrderByDescending(node => node.InnerText.Length)
                .FirstOrDefault();

            return mainContentNode != null ? mainContentNode.InnerText : "Content not found";
        }
    }
}
