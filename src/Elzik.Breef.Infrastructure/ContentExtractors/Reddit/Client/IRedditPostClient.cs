namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client
{
    public interface IRedditPostClient
    {
        Task<RedditPost> GetPost(string postId);
    }
}
