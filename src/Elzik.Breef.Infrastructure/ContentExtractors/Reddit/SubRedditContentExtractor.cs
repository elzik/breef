using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit;

public class SubredditContentExtractor
    (ISubredditClient subredditClient, IHttpClientFactory httpClientFactory, IOptions<RedditOptions> redditOptions)
    : IContentExtractor, ISubredditImageExtractor
{
    private const char UrlPathSeparator = '/';
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly RedditOptions _redditOptions = redditOptions.Value;

    public bool CanHandle(string webPageUrl)
    {
        if (!Uri.TryCreate(webPageUrl, UriKind.Absolute, out Uri? webPageUri))
            return false;

        var requestDomain = webPageUri.Host;
        
        if (!_redditOptions.AllDomains.Any(allowedDomain => 
            requestDomain.Equals(allowedDomain, StringComparison.OrdinalIgnoreCase)))
            return false;

        var segments = webPageUri.AbsolutePath.Trim(UrlPathSeparator).Split(UrlPathSeparator);

        return 
            segments.Length == 2 && 
            segments[0].Equals("r", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Extract> ExtractAsync(string webPageUrl)
    {
        var webPageUri = new Uri(webPageUrl.EndsWith(UrlPathSeparator) ? webPageUrl : webPageUrl + UrlPathSeparator, UriKind.Absolute);
        var webPageParts = webPageUri.AbsolutePath.Trim(UrlPathSeparator).Split(UrlPathSeparator);
        var subredditName = webPageParts[^1];
        
        var newInSubreddit = await subredditClient.GetNewInSubreddit(subredditName);
        var jsonContent = JsonSerializer.Serialize(newInSubreddit);
        var imageUrl = await ExtractImageUrlAsync(webPageUri);

        return new Extract($"New in r/{subredditName}", jsonContent, imageUrl);
    }

    public async Task<string> GetSubredditImageUrlAsync(string subredditName)
    {
        var subRedditBaseUri = new Uri($"{_redditOptions.DefaultBaseAddress}/r/{subredditName}/");
        return await ExtractImageUrlAsync(subRedditBaseUri);
    }

    private async Task<string> ExtractImageUrlAsync(Uri subRedditBaseUri)
    {
      Uri subRedditAboutUri = new(subRedditBaseUri, "about.json");
        var httpClient = _httpClientFactory.CreateClient("BreefDownloader");
        var jsonContent = await httpClient.GetStringAsync(subRedditAboutUri.AbsoluteUri);

        string[] imageKeys = ["banner_background_image", "banner_img", "mobile_banner_image", "icon_img", "community_icon"];

        using var doc = JsonDocument.Parse(jsonContent);
        var data = doc.RootElement.GetProperty("data");

     foreach (var imageKey in imageKeys)
        {
  if (data.TryGetProperty(imageKey, out var prop))
    {
  var imageUrl = prop.GetString();
        if (!string.IsNullOrWhiteSpace(imageUrl) && 
   Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) &&
       (uri.Scheme == "http" || uri.Scheme == "https"))
                {
  var client = _httpClientFactory.CreateClient("BreefDownloader");
             var response = await client.GetAsync(imageUrl);
           if (response.IsSuccessStatusCode)
         {
                 return imageUrl;
       }
      }
            }
        }

        return _redditOptions.FallbackImageUrl;
  }
}
