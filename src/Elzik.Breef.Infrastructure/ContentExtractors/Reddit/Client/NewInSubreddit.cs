using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class NewInSubreddit
{
    [JsonPropertyName("data")]
    public ListingData? Data { get; set; }
}

public class ListingData
{
    [JsonPropertyName("children")]
    public List<Child>? Children { get; set; }
}

public class Child
{
    [JsonPropertyName("data")]
    public PostData? Data { get; set; }
}

public class PostData
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("selftext")]
    public string? SelfText { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
