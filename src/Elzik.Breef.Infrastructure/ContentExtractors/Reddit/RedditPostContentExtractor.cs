using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit;

public class RedditPostContentExtractor(
    IRedditPostClient redditPostClient,
    ISubredditImageExtractor subredditImageExtractor,
    IOptions<RedditOptions> redditOptions) : IContentExtractor
{
    private readonly RedditOptions _redditOptions = redditOptions.Value;

    public bool CanHandle(string webPageUrl)
    {
        if (!Uri.TryCreate(webPageUrl, UriKind.Absolute, out Uri? webPageUri))
            return false;

        var requestDomain = webPageUri.Host;
        
        if (!_redditOptions.AllDomains.Any(allowedDomain => 
            requestDomain.Equals(allowedDomain, StringComparison.OrdinalIgnoreCase)))
            return false;

        var segments = webPageUri.AbsolutePath.Trim('/').Split('/');

        return 
            (segments.Length == 4 || segments.Length == 5) && 
            segments[0].Equals("r", StringComparison.OrdinalIgnoreCase) && 
            segments[2].Equals("comments", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Extract> ExtractAsync(string webPageUrl)
    {
        if (!Uri.TryCreate(webPageUrl, UriKind.Absolute, out Uri? webPageUri))
            throw new InvalidOperationException($"Invalid URL format: '{webPageUrl}'. " +
                $"URL must be a valid absolute URI.");

        var requestDomain = webPageUri.Host;
        
        if (!_redditOptions.AllDomains.Any(allowedDomain => 
            requestDomain.Equals(allowedDomain, StringComparison.OrdinalIgnoreCase)))
        {
            var supportedDomains = string.Join(", ", _redditOptions.AllDomains);
            throw new InvalidOperationException($"Unsupported domain: '{requestDomain}'. " +
                $"Supported domains: {supportedDomains}");
        }

        var segments = webPageUri.AbsolutePath.Trim('/').Split('/');

        if (!((segments.Length == 4 || segments.Length == 5) &&
            segments[0].Equals("r", StringComparison.OrdinalIgnoreCase) &&
            segments[2].Equals("comments", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Unsupported Reddit URL format: '{webPageUrl}'. " +
                $"Expected format: 'https://[reddit-domain]/r/[subreddit]/comments/[postId]' " +
                $"or 'https://[reddit-domain]/r/[subreddit]/comments/[postId]/[title]'.");
        }

        var postId = segments[3];
        var post = await redditPostClient.GetPost(postId);

        if (string.IsNullOrWhiteSpace(post.Post.ImageUrl))
        {
            var subredditName = segments[1];
            post.Post.ImageUrl = await subredditImageExtractor.GetSubredditImageUrlAsync(subredditName);
        }

        var postJson = JsonSerializer.Serialize(post);

        return new Extract(post.Post.Title, postJson, post.Post.ImageUrl);
    }
}
