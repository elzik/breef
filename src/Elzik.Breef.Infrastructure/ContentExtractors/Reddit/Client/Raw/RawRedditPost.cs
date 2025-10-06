using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;

public class RawRedditPost : List<RawRedditListing>
{
}

public class RawRedditListing
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("data")]
    public RawRedditListingData Data { get; set; } = new();
}

public class RawRedditListingData
{
    [JsonPropertyName("after")]
    public string? After { get; set; }

    [JsonPropertyName("before")]
    public string? Before { get; set; }

    [JsonPropertyName("children")]
    public List<RawRedditChild> Children { get; set; } = [];
}

public class RawRedditChild
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("data")]
    public RawRedditCommentData Data { get; set; } = new();
}

public class RawRedditCommentData
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("selftext")]
    public string? SelfText { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("subreddit")]
    public string? Subreddit { get; set; }

    [JsonPropertyName("created_utc")]
    [JsonConverter(typeof(RedditDateTimeConverter))]
    public DateTime CreatedUtc { get; set; }

    [JsonPropertyName("replies")]
    public object? Replies { get; set; } // Use object to handle both RawRedditListing and empty string cases

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("url_overridden_by_dest")]
    public string? UrlOverriddenByDest { get; set; }

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    [JsonPropertyName("preview")]
    public RawRedditPreview? Preview { get; set; }

    [JsonPropertyName("is_gallery")]
    public bool IsGallery { get; set; }

    [JsonPropertyName("media_metadata")]
    public Dictionary<string, RawRedditMediaMetadata>? MediaMetadata { get; set; }

    [JsonPropertyName("gallery_data")]
    public RawRedditGalleryData? GalleryData { get; set; }

    [JsonIgnore]
    public string? Content => Body ?? SelfText;
}

public class RawRedditPreview
{
    [JsonPropertyName("images")]
    public List<RawRedditPreviewImage>? Images { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

public class RawRedditPreviewImage
{
    [JsonPropertyName("source")]
    public RawRedditImageSource? Source { get; set; }

    [JsonPropertyName("resolutions")]
    public List<RawRedditImageSource>? Resolutions { get; set; }
}

public class RawRedditImageSource
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class RawRedditMediaMetadata
{
    [JsonPropertyName("s")]
    public RawRedditImageSource? Source { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("e")]
    public string? Extension { get; set; }

    [JsonPropertyName("m")]
    public string? MimeType { get; set; }
}

public class RawRedditGalleryData
{
    [JsonPropertyName("items")]
    public List<RawRedditGalleryItem>? Items { get; set; }
}

public class RawRedditGalleryItem
{
    [JsonPropertyName("media_id")]
    public string? MediaId { get; set; }

    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; set; }
}
