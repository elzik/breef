using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class RawNewInSubreddit
{
    [JsonPropertyName("data")]
    public RawListingData? Data { get; set; }
}

public class RawListingData
{
    [JsonPropertyName("children")]
    public List<RawChild>? Children { get; set; }
}

public class RawChild
{
    [JsonPropertyName("data")]
    public RawPostData? Data { get; set; }
}

public class RawPostData
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("selftext")]
    public string? SelfText { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
