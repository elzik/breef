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
    public RedditListingData Data { get; set; } = new();
}

public class RedditListingData
{
    [JsonPropertyName("after")]
    public string? After { get; set; }

    [JsonPropertyName("before")]
    public string? Before { get; set; }

    [JsonPropertyName("children")]
    public List<RedditChild> Children { get; set; } = [];
}

public class RedditChild
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("data")]
    public RedditCommentData Data { get; set; } = new();
}

public class RedditCommentData
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
    [JsonConverter(typeof(RawRedditRepliesConverter))]
    public RawRedditListing Replies { get; set; } = new RawRedditListing
    {
        Data = new RedditListingData
        {
            Children = []
        }
    };

    [JsonIgnore]
    public string? Content => Body ?? SelfText;
}