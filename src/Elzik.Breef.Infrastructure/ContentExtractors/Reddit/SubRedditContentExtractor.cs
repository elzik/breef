using Elzik.Breef.Domain;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit
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
            var subRedditBaseUri = webPageUri.ToString().EndsWith('/')
                ? webPageUri
                : new Uri(webPageUri.ToString() + "/");
            Uri subRedditNewPostsUri = new(subRedditBaseUri, "new.json");

            var subredditName = webPageUri.AbsolutePath.Trim('/').Split('/').Last();
            var jsonContent = await httpDownloader.DownloadAsync(subRedditNewPostsUri.AbsoluteUri);
            var imageUrl = await ExtractImageUrlAsync(subRedditBaseUri);

            return new Extract($"New in r/{subredditName}", jsonContent, imageUrl);
        }

        private async Task<string> ExtractImageUrlAsync(Uri subRedditBaseUri)
        {
            Uri subRedditAboutUri = new(subRedditBaseUri, "about.json");
            var jsonContent = await httpDownloader.DownloadAsync(subRedditAboutUri.AbsoluteUri);

            string[] imageKeys = ["banner_background_image", "banner_img", "mobile_banner_image", "icon_img", "community_icon"];

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

            return "https://redditinc.com/hubfs/Reddit%20Inc/Brand/Reddit_Lockup_Logo.svg";
        }
    }
}
