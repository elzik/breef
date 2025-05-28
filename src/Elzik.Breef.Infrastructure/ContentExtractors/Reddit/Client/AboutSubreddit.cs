using System.Text.Json.Serialization;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class AboutSubreddit
{
    [JsonPropertyName("data")]
    public AboutSubredditData? Data { get; set; }
}

public class AboutSubredditData
{
    [JsonPropertyName("public_description")]
    public string? PublicDescription { get; set; }

    [JsonPropertyName("icon_img")]
    public string? IconImg { get; set; }

    [JsonPropertyName("banner_img")]
    public string? BannerImg { get; set; }

    [JsonPropertyName("banner_background_image")]
    public string? BannerBackgroundImage { get; set; }

    [JsonPropertyName("mobile_banner_image")]
    public string? MobileBannerImage { get; set; }

    [JsonPropertyName("community_icon")]
    public string? CommunityIcon { get; set; }
}