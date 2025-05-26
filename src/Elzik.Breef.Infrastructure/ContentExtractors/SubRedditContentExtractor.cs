using Elzik.Breef.Domain;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.ContentExtractors
{
    public class SubRedditContentExtractor(IHttpDownloader httpDownloader) : IContentExtractor
    {
        public bool CanHandle(string webPageUrl)
        {
            if (!Uri.TryCreate(webPageUrl, UriKind.Absolute, out Uri? webPageUri))
                return false;

            var host = webPageUri.Host;
            if (!host.Equals("reddit.com", StringComparison.OrdinalIgnoreCase) &&
                !host.Equals("www.reddit.com", StringComparison.OrdinalIgnoreCase))
                return false;

            var segments = webPageUri.AbsolutePath.Trim('/').Split('/');

            return 
                segments.Length == 2 && 
                segments[0].Equals("r", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<Extract> ExtractAsync(string webPageUrl)
        {
            Uri webPageUri = new(webPageUrl);
            Uri jsonUri = new(webPageUri, "new.json");

            var jsonContent = await httpDownloader.DownloadAsync(jsonUri.AbsoluteUri);


            var subredditName = webPageUri.AbsolutePath.Trim('/').Split('/').Last();
            var imageUrl = await ExtractImageUrlAsync(jsonContent);


            return new Extract($"New in r/{subredditName}", jsonContent, imageUrl);
        }

        private async Task<string> ExtractImageUrlAsync(string jsonContent)
        {
            string[] imageKeys = ["icon_img", "community_icon", "banner_background_image", "banner_img", "mobile_banner_image"];

            using var doc = JsonDocument.Parse(jsonContent);
            var data = doc.RootElement.GetProperty("data");

            foreach (var imageKey in imageKeys)
            {
                if (data.TryGetProperty(imageKey, out var prop))
                {
                    var imageUrl = prop.GetString();
                    if (imageUrl != null && await httpDownloader.TryGet(imageUrl))
                    {
                        return imageUrl;
                    }
                }
            }

            return "https://www.redditstatic.com/icon.png";
        }
    }
}
