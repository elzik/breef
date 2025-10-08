using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RedditPostJsonExample
{
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    public RedditPostJsonExample()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public void RedditPost_SerializesToJson_ProducesExpectedFormat()
    {
        // Arrange
        var redditPost = new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = "1kqiwzc",
                Title = "Should I take a .NET developer program if I want to freelance?",
                Author = "melvman1",
                Subreddit = "r/learnprogramming",
                Score = 15,
                Content = "I am just about to enter the programming world, and want to become a software engineer...",
                CreatedUtc = new DateTime(2025, 5, 19, 18, 18, 5, DateTimeKind.Utc)
            },
            Comments =
            [
                new() {
                    Id = "mt7aaf6",
                    Author = "CodeRadDesign",
                    Score = 125,
                    Content = "not really.\n\nas someone who's been freelance on and off for 30 years...",
                    CreatedUtc = new DateTime(2025, 5, 19, 19, 0, 0, DateTimeKind.Utc),
                    Replies = []
                },
                new() {
                    Id = "mt606l6",
                    Author = "[deleted]",
                    Score = 2,
                    Content = "[deleted]",
                    CreatedUtc = new DateTime(2025, 5, 19, 20, 0, 0, DateTimeKind.Utc),
                    Replies =
                    [
                        new() {
                            Id = "mt60jnv",
                            Author = "melvman1",
                            Score = 1,
                            Content = "I am willing to work at the company...",
                            CreatedUtc = new DateTime(2025, 5, 19, 20, 30, 0, DateTimeKind.Utc),
                            Replies = []
                        }
                    ]
                }
            ]
        };

        // Act
        var json = JsonSerializer.Serialize(redditPost, _jsonSerializerOptions);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();

        // Verify structure
        json.ShouldContain("\"post\":");
        json.ShouldContain("\"comments\":");
        json.ShouldContain("\"id\": \"1kqiwzc\"");
        json.ShouldContain("\"title\": \"Should I take a .NET developer program if I want to freelance?\"");
        json.ShouldContain("\"author\": \"melvman1\"");
        json.ShouldContain("\"subreddit\": \"r/learnprogramming\"");
        json.ShouldContain("\"score\": 15");
        json.ShouldContain("\"replies\":");

        // Print the JSON for demonstration
        System.Console.WriteLine("Reddit Post JSON Structure:");
        System.Console.WriteLine(json);
    }
}