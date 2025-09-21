using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

public class RawRedditPostTransformer
{
    public RedditPost Transform(RawRedditPost rawRedditPost)
    {
        if (rawRedditPost.Count < 2)
            throw new ArgumentException("Reddit post must have at least 2 listings (post and comments)", nameof(rawRedditPost));

        var postListing = rawRedditPost[0];
        var commentsListing = rawRedditPost[1];

        if (postListing.Data.Children.Count == 0)
            throw new ArgumentException("Post listing must contain at least one child", nameof(rawRedditPost));

        var mainPostData = postListing.Data.Children[0].Data;

        var redditPost = new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = mainPostData.Id ?? string.Empty,
                Title = mainPostData.Title ?? throw new InvalidOperationException("Reddit post must have a title"),
                Author = mainPostData.Author ?? string.Empty,
                Subreddit = mainPostData.Subreddit ?? string.Empty,
                Score = mainPostData.Score,
                Content = mainPostData.Content ?? string.Empty,
                CreatedUtc = mainPostData.CreatedUtc
            },
            Comments = TransformComments(commentsListing.Data.Children)
        };

        return redditPost;
    }

    private List<RedditComment> TransformComments(List<RedditChild> children)
    {
        var comments = new List<RedditComment>();

        foreach (var child in children)
        {
            if (child.Kind == "t1") // Comment type
            {
                var comment = new RedditComment
                {
                    Id = child.Data.Id ?? string.Empty,
                    Author = child.Data.Author ?? string.Empty,
                    Score = child.Data.Score,
                    Content = child.Data.Content ?? string.Empty,
                    CreatedUtc = child.Data.CreatedUtc,
                    Replies = TransformComments(child.Data.Replies.Data.Children)
                };

                comments.Add(comment);
            }
        }

        return comments;
    }
}