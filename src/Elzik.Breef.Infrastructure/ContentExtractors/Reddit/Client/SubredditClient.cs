using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class SubredditClient(IRawSubredditClient rawSubredditClient, IRawNewInSubredditTransformer transformer) : ISubredditClient
{
    public async Task<NewInSubreddit> GetNewInSubreddit(string subRedditName)
    {
        var rawNewInSubreddit = await rawSubredditClient.GetNewInSubreddit(subRedditName);

        return await transformer.Transform(rawNewInSubreddit);
    }
}