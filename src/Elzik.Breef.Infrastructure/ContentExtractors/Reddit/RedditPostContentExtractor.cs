using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit;

public class RedditPostContentExtractor(
    IRedditPostClient redditPostClient,
    ISubredditImageExtractor subredditImageExtractor) : IContentExtractor
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
            (segments.Length == 4 || segments.Length == 5) && 
            segments[0].Equals("r", StringComparison.OrdinalIgnoreCase) && 
            segments[2].Equals("comments", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Extract> ExtractAsync(string webPageUrl)
    {
        if (!Uri.TryCreate(webPageUrl, UriKind.Absolute, out Uri? webPageUri))
            throw new InvalidOperationException($"Invalid URL format: '{webPageUrl}'. " +
                $"URL must be a valid absolute URI.");

        var host = webPageUri.Host;
        if (!host.Equals("reddit.com", StringComparison.OrdinalIgnoreCase) &&
            !host.Equals("www.reddit.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unsupported host: '{host}'. " +
                $"Only reddit.com and www.reddit.com are supported.");

        var segments = webPageUri.AbsolutePath.Trim('/').Split('/');

        if (!((segments.Length == 4 || segments.Length == 5) &&
            segments[0].Equals("r", StringComparison.OrdinalIgnoreCase) &&
            segments[2].Equals("comments", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Unsupported Reddit URL format: '{webPageUrl}'. " +
                $"Expected format: 'https://reddit.com/r/[subreddit]/comments/[postId]' " +
                $"or 'https://reddit.com/r/[subreddit]/comments/[postId]/[title]'.");
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
