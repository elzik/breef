namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class RawNewInSubredditTransformer(IRedditPostClient redditPostClient) : IRawNewInSubredditTransformer
{
    public async Task<NewInSubreddit> Transform(RawNewInSubreddit rawNewInSubreddit)
    {
        ArgumentNullException.ThrowIfNull(rawNewInSubreddit);

        var newInSubreddit = new NewInSubreddit();

        if (rawNewInSubreddit.Data?.Children == null || rawNewInSubreddit.Data.Children.Count == 0)
        {
            return newInSubreddit;
        }

        var postIds = rawNewInSubreddit.Data.Children
            .Where(child => child.Data?.Id != null)
            .Select(child => child.Data!.Id!)
            .ToList();

        var postTasks = postIds.Select(id => redditPostClient.GetPost(id));
        var posts = await Task.WhenAll(postTasks);

        newInSubreddit.Posts.AddRange(posts);

        return newInSubreddit;
    }
}