using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

public class RawRedditPostTransformer
{
    public RedditPost Transform(RawRedditPost rawRedditPost)
    {
        if (rawRedditPost.Count < 2)
            throw new ArgumentException("Reddit post must have at least 2 listings (post and comments)", nameof(rawRedditPost));

        var postListing = rawRedditPost[0];
        var commentsListing = rawRedditPost[1];

        var postChildren = postListing.Data?.Children;
        if (postChildren == null || postChildren.Count == 0)
            throw new ArgumentException("Post listing must contain at least one child", nameof(rawRedditPost));

        var mainPostData = postChildren[0].Data;

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
            Comments = TransformComments(commentsListing)
        };

        return redditPost;
    }

    private List<RedditComment> TransformComments(List<RawRedditChild> children)
    {
        var comments = new List<RedditComment>();

        foreach (var child in children)
        {
            if (child.Kind == "t1")
            {
                var comment = new RedditComment
                {
                    Id = child.Data.Id ?? string.Empty,
                    Author = child.Data.Author ?? string.Empty,
                    Score = child.Data.Score,
                    Content = child.Data.Content ?? string.Empty,
                    CreatedUtc = child.Data.CreatedUtc,
                    Replies = TransformComments(child.Data.Replies)
                };

                comments.Add(comment);
            }
        }

        return comments;
    }

    private List<RedditComment> TransformComments(object? replies)
    {
        if (replies == null)
            return [];

        if (replies is string stringReply && stringReply == "")
            return [];

        if (replies is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
                return [];

            if (jsonElement.ValueKind == JsonValueKind.String && jsonElement.GetString() == "")
                return [];

            try
            {
                var deserializedListing = JsonSerializer.Deserialize<RawRedditListing>(jsonElement.GetRawText());
                return TransformComments(deserializedListing);
            }
            catch
            {
                return [];
            }
        }

        if (replies is RawRedditListing listing)
            return TransformComments(listing);

        return [];
    }

    private List<RedditComment> TransformComments(RawRedditListing? replies)
    {
        if (replies == null)
            return [];

        if (replies.Data == null)
            return [];

        if (replies.Data.Children == null)
            return [];

        return TransformComments(replies.Data.Children);
    }
}