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

    private List<RedditComment> TransformComments(List<RawRedditChild> children)
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
                    Replies = TransformComments(child.Data.Replies)
                };

                comments.Add(comment);
            }
        }

        return comments;
    }

    private List<RedditComment> TransformComments(object? replies)
    {
        // Handle null replies
        if (replies == null)
            return [];

        // Handle empty string replies (Reddit API quirk)
        if (replies is string stringReply && stringReply == "")
            return [];

        // Handle JsonElement (when deserialized as object)
        if (replies is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
                return [];

            if (jsonElement.ValueKind == JsonValueKind.String && jsonElement.GetString() == "")
                return [];

            // Try to deserialize as RawRedditListing
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

        // Handle direct RawRedditListing object
        if (replies is RawRedditListing listing)
            return TransformComments(listing);

        // Unknown type, return empty list
        return [];
    }

    private List<RedditComment> TransformComments(RawRedditListing? replies)
    {
        // Handle null replies
        if (replies == null)
            return [];

        // Handle missing Data property
        if (replies.Data == null)
            return [];

        // Handle missing Children property
        if (replies.Data.Children == null)
            return [];

        // Transform the children
        return TransformComments(replies.Data.Children);
    }
}