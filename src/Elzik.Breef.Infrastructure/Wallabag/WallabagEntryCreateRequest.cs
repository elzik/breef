using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.Wallabag
{
    public class WallabagEntryCreateRequest
    {
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }

        [JsonPropertyName("tags")]
        public string? Tags { get; set; }

        [JsonPropertyName("archive")]
        public int Archive { get; set; } = 0;

        [JsonPropertyName("starred")]
        public int Starred { get; set; } = 0;

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("preview_picture")]
        public string? PreviewPicture { get; set; }

        [JsonPropertyName("published_at")]
        public string? PublishedAt { get; set; }

        [JsonPropertyName("authors")]
        public string? Authors { get; set; }

        [JsonPropertyName("public")]
        public int Public { get; set; } = 0;

        [JsonPropertyName("origin_url")]
        public string? OriginUrl { get; set; }
    }
}