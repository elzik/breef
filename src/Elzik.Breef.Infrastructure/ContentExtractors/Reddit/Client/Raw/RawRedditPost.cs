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

    [JsonIgnore]
    public string? Content => Body ?? SelfText;
}