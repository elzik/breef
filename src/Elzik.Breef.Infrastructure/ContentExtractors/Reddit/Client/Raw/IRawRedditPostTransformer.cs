namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

public interface IRawRedditPostTransformer
{
    RedditPost Transform(RawRedditPost rawRedditPost);
}