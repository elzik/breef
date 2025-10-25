using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class RedditPostClient(IRawRedditPostClient redditPostClient, IRawRedditPostTransformer transformer) : IRedditPostClient
{
    public async Task<RedditPost> GetPost(string postId)
    {
        var redditPost = await redditPostClient.GetPost(postId);
        return transformer.Transform(redditPost);
    }
}