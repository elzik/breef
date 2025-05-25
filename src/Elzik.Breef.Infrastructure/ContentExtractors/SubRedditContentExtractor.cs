using Elzik.Breef.Domain;

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
            var jsonUri = new Uri(new Uri(webPageUrl), "new.json");

            var json = await httpDownloader.DownloadAsync(jsonUri.AbsoluteUri);

            // Image
            //https://www.reddit.com/r/{subreddit}/about.json
            // The response will contain a community_icon or icon_img field, which usually holds the avatar URL.

            return new Extract("TBA", json, "TBA");
        }
    }
}
