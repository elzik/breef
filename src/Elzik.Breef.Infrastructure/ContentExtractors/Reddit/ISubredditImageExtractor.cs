namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit;

public interface ISubredditImageExtractor
{
    Task<string> GetSubredditImageUrlAsync(string subredditName);
}