using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elzik.Breef.Domain
{
    public class ContentExtractor(IHttpClient httpClient)
    {
        public async Task<string> Extract(string webPageUrl)
        {
            var html = await httpClient.GetStringAsync(webPageUrl);

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
