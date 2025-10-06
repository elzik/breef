using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class RedditPostClient : IRedditPostClient
{
    private readonly IRawRedditPostClient _redditPostClient;
    private readonly IRawRedditPostTransformer _transformer;

    public RedditPostClient(IRawRedditPostClient redditPostClient, IRawRedditPostTransformer transformer)
    {
        _redditPostClient = redditPostClient;
        _transformer = transformer;
    }

    public async Task<RedditPost> GetPost(string postId)
    {
        var redditPost = await _redditPostClient.GetPost(postId);
        return _transformer.Transform(redditPost);
    }
}