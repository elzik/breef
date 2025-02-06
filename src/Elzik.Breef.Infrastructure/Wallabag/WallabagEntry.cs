using Refit;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.Wallabag
{
    public class WallabagEntry
    {
        [JsonPropertyName("is_archived")]
        public required int IsArchived { get; set; }

        [JsonPropertyName("is_starred")]
        public required int IsStarred { get; set; }

        [JsonPropertyName("user_name")]
        public required string UserName { get; set; }

        [JsonPropertyName("user_email")]
        public required string UserEmail { get; set; }

        [JsonPropertyName("user_id")]
        public required int UserId { get; set; }

        [JsonPropertyName("tags")]
        public required List<Tag> Tags { get; set; }

        [JsonPropertyName("is_public")]
        public required bool IsPublic { get; set; }

        [JsonPropertyName("id")]
        public required int Id { get; set; }

        [JsonPropertyName("uid")]
        public string? Uid { get; set; }

        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("hashed_url")]
        public required string HashedUrl { get; set; }

        [JsonPropertyName("origin_url")]
        public string? OriginUrl { get; set; }

        [JsonPropertyName("given_url")]
        public required string GivenUrl { get; set; }

        [JsonPropertyName("hashed_given_url")]
        public required string HashedGivenUrl { get; set; }

        [JsonPropertyName("archived_at")]
        [JsonConverter(typeof(WallabagDateTimeConverter))]
        public DateTime? ArchivedAt { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }

        [JsonPropertyName("created_at")]
        [JsonConverter(typeof(WallabagDateTimeConverter))]
        public required DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        [JsonConverter(typeof(WallabagDateTimeConverter))]
        public required DateTime UpdatedAt { get; set; }

        [JsonPropertyName("published_at")]
        [JsonConverter(typeof(WallabagDateTimeConverter))]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("published_by")]
        public required List<string> PublishedBy { get; set; }

        [JsonPropertyName("starred_at")]
        [JsonConverter(typeof(WallabagDateTimeConverter))]
        public DateTime? StarredAt { get; set; }

        [JsonPropertyName("annotations")]
        public required List<string> Annotations { get; set; }

        [JsonPropertyName("mimetype")]
        public string? MimeType { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("reading_time")]
        public required int ReadingTime { get; set; }

        [JsonPropertyName("domain_name")]
        public required string DomainName { get; set; }

        [JsonPropertyName("preview_picture")]
        public string? PreviewPicture { get; set; }

        [JsonPropertyName("http_status")]
        public string? HttpStatus { get; set; }

        [JsonPropertyName("headers")]
        public required Dictionary<string, string> Headers { get; set; }


        [JsonPropertyName("_links")]
        public required Links Links { get; set; }
    }

    public class Links
    {
        [JsonPropertyName("self")]
        public required Self Self { get; set; }
    }

    public class Self
    {
        [JsonPropertyName("href")]
        public required string Href { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("label")]
        public required string Label { get; set; }

        [JsonPropertyName("slug")]
        public required string Slug { get; set; }
    }
}
