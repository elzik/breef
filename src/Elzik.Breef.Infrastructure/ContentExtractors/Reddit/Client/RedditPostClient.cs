using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class RedditPostClient : IRedditPostClient
{
    private readonly IRawRedditPostClient _redditPostClient;
    private readonly RawRedditPostTransformer _transformer;

    public RedditPostClient(IRawRedditPostClient redditPostClient, RawRedditPostTransformer transformer)
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