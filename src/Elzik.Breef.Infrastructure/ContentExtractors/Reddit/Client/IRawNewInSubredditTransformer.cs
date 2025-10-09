namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public interface IRawNewInSubredditTransformer
{
    Task<NewInSubreddit> Transform(RawNewInSubreddit rawNewInSubreddit);
}