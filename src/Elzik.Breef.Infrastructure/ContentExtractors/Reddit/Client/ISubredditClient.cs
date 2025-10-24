namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public interface ISubredditClient
{
    Task<NewInSubreddit> GetNewInSubreddit(string subRedditName);
}
