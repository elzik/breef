using Refit;

namespace Elzik.Breef.Infrastructure.Wallabag
{
    public class WallabagEntryCreateRequest
    {
        [AliasAs("url")]
        public required string Url { get; set; }

        [AliasAs("title")]
        public string? Title { get; set; }

        [AliasAs("content")]
        public required string Content { get; set; }

        [AliasAs("tags")]
        public string? Tags { get; set; }

        [AliasAs("archive")]
        public int Archive { get; set; } = 0;

        [AliasAs("starred")]
        public int Starred { get; set; } = 0;

        [AliasAs("language")]
        public string? Language { get; set; }

        [AliasAs("preview_picture")]
        public string? PreviewPicture { get; set; }

        [AliasAs("published_at")]
        public string? PublishedAt { get; set; }

        [AliasAs("authors")]
        public string? Authors { get; set; }

        [AliasAs("public")]
        public int Public { get; set; } = 0;

        [AliasAs("origin_url")]
        public string? OriginUrl { get; set; }
    }
}