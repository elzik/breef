using HtmlAgilityPack;

namespace Elzik.Breef.Domain;

public class ContentExtractor(IWebPageDownloader httpClient) : IContentExtractor
{
    public async Task<Extract> ExtractAsync(string webPageUrl)
    {
        var html = await httpClient.DownloadAsync(webPageUrl);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var content = GetContentFromDocument(htmlDocument);
        var title = GetTitleFromDocument(htmlDocument, webPageUrl);

        return new Extract(title, content);
    }

    private static string GetContentFromDocument(HtmlDocument htmlDocument)
    {
        var mainContentNode = htmlDocument.DocumentNode
                    .SelectNodes("//div|//article|//p")
                    ?.OrderByDescending(node => node.InnerText.Length)
                    .FirstOrDefault();

        var content = mainContentNode != null
            ? mainContentNode.InnerText : "Content not found.";

        return content;
    }

    private static string GetTitleFromDocument(HtmlDocument htmlDocument, string defaultWhenMissing)
    {
        var title = string.Empty;

        var titleMetaTag = htmlDocument.DocumentNode
            .SelectSingleNode("//meta[@property='og:title']");
        if (titleMetaTag != null)
        {
            title = HtmlEntity.DeEntitize(titleMetaTag.GetAttributeValue("content", null));
        }

        if (string.IsNullOrEmpty(title))
        {
            var titleNode = htmlDocument.DocumentNode
                .SelectSingleNode("//title");
            title = titleNode != null
                ? HtmlEntity.DeEntitize(titleNode.InnerText)
                : defaultWhenMissing;
        }

        return title;
    }
}
