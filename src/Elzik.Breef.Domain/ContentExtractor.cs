using HtmlAgilityPack;

namespace Elzik.Breef.Domain;

public class ContentExtractor(IWebPageDownloader httpClient) : IContentExtractor
{
    public async Task<Extract> ExtractAsync(string webPageUrl)
    {
        var html = await httpClient.DownloadAsync(webPageUrl);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var content = GetContent(htmlDocument);
        var title = GetTitle(htmlDocument, webPageUrl);
        var largestImageUrl = GetLargestImageUrl(htmlDocument);


        return new Extract(title, content, largestImageUrl);
    }

    private static string GetContent(HtmlDocument htmlDocument)
    {
        var mainContentNode = htmlDocument.DocumentNode
                    .SelectNodes("//div|//article|//p")
                    ?.OrderByDescending(node => node.InnerText.Length)
                    .FirstOrDefault();

        var content = mainContentNode != null
            ? mainContentNode.InnerText.Trim() : "Content not found.";

        return content;
    }

    private static string GetTitle(HtmlDocument htmlDocument, string defaultWhenMissing)
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

    private static string? GetLargestImageUrl(HtmlDocument htmlDocument)
    {
        var imageNodes = htmlDocument.DocumentNode.SelectNodes("//img");
        if (imageNodes == null || imageNodes.Count == 0)
        {
            return null;
        }

        var imageNodesSortedBySize = imageNodes
            .Select(node => new
            {
                Node = node,
                Width = int.TryParse(node.GetAttributeValue("width", "0"), out var width) ? width : 0,
                Height = int.TryParse(node.GetAttributeValue("height", "0"), out var height) ? height : 0
            })
            .OrderByDescending(img => img.Width * img.Height);

        var largestImageNode = imageNodesSortedBySize.FirstOrDefault()?.Node;

        return largestImageNode?.GetAttributeValue("src", null);
    }
}
