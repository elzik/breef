using System.Text.Json;
using System.Web;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

public class RawRedditPostTransformer : IRawRedditPostTransformer
{
    public RedditPost Transform(RawRedditPost rawRedditPost)
    {
        ArgumentNullException.ThrowIfNull(rawRedditPost);
        if (rawRedditPost.Count < 2)
            throw new ArgumentException("Reddit post must have at least 2 listings (post and comments)", nameof(rawRedditPost));

        var postListing = rawRedditPost[0];
        var commentsListing = rawRedditPost[1];

        var postChildren = postListing.Data?.Children;
        if (postChildren == null || postChildren.Count == 0)
            throw new ArgumentException("Post listing must contain at least one child", nameof(rawRedditPost));

        var mainPostData = postChildren[0].Data;
        var bestImage = ExtractBestImage(mainPostData);

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
                CreatedUtc = mainPostData.CreatedUtc,
                ImageUrl = bestImage
            },
            Comments = TransformComments(commentsListing)
        };

        return redditPost;
    }

    private static string? ExtractBestImage(RawRedditCommentData postData)
    {
        // 1. Gallery images (highest priority) - pick the first/largest
        if (postData.IsGallery && postData.GalleryData?.Items != null && postData.MediaMetadata != null)
        {
            var bestGalleryImage = postData.GalleryData.Items
                .Where(item => item.MediaId != null && postData.MediaMetadata.ContainsKey(item.MediaId))
                .Select(item => postData.MediaMetadata[item.MediaId!])
                .Where(metadata => metadata.Status == "valid" && metadata.Source?.Url != null)
                .OrderByDescending(metadata => metadata.Source!.Width * metadata.Source.Height)
                .FirstOrDefault();

            if (bestGalleryImage?.Source?.Url != null)
            {
                return HttpUtility.HtmlDecode(bestGalleryImage.Source.Url);
            }
        }

        // 2. Preview images (high priority) - pick the largest
        if (postData.Preview?.Images != null)
        {
            var bestPreviewImage = postData.Preview.Images
                .Where(img => img.Source?.Url != null)
                .OrderByDescending(img => img.Source!.Width * img.Source.Height)
                .FirstOrDefault();

            if (bestPreviewImage?.Source?.Url != null)
            {
                return HttpUtility.HtmlDecode(bestPreviewImage.Source.Url);
            }
        }

        // 3. Direct image URL
        var directUrl = postData.UrlOverriddenByDest ?? postData.Url;
        if (IsImageUrl(directUrl))
        {
            return directUrl;
        }

        // 4. Thumbnail (last resort)
        if (!string.IsNullOrEmpty(postData.Thumbnail) && 
            postData.Thumbnail != "self" && 
            postData.Thumbnail != "default" && 
            postData.Thumbnail != "nsfw" &&
            IsImageUrl(postData.Thumbnail))
        {
            return postData.Thumbnail;
        }

        return null;
    }

    private static bool IsImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        var extension = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg";
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