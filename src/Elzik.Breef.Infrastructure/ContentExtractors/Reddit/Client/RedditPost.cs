namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

public class RedditPost
{
    public RedditPostContent Post { get; set; } = new();
    public List<RedditComment> Comments { get; set; } = [];
}

public class RedditPostContent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Subreddit { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public string? ImageUrl { get; set; }
    public string PostUrl { get; set; } = string.Empty;
}

public class RedditComment
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public List<RedditComment> Replies { get; set; } = [];
    public string PostUrl { get; set; } = string.Empty;
}