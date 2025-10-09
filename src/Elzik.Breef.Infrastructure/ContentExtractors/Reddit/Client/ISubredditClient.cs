using Refit;


namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public interface ISubredditClient
{
    [Get("/r/{subRedditName}/new.json")]
    [Headers("User-Agent: breef/1.0.0 (https://github.com/elzik/breef)")]
    Task<RawNewInSubreddit> GetNewInSubreddit(string subRedditName);

    [Get("/r/{subRedditName}/about.json")]
    [Headers("User-Agent: breef/1.0.0 (https://github.com/elzik/breef)")]
    Task<AboutSubreddit> GetAboutSubreddit(string subRedditName);
}
