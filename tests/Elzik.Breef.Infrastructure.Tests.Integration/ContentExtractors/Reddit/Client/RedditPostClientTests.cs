using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Refit;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors.Reddit.Client;

public class RedditPostClientTests
{
    private static bool IsRunningInGitHubWorkflow => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

    [SkippableFact]
    public async Task GetPost_ValidPostId_ReturnsRedditPost()
    {
        // Arrange
        Skip.If(IsRunningInGitHubWorkflow, "Skipped because requests to reddit.com from GitHub workflows are " +
            "always blocked meaning this test case always fails. This must be run locally instead.");
        var client = RestService.For<IRedditPostClient>("https://www.reddit.com/");
        var postId = "1kqiwzc";

        // Act
        var redditPost = await client.GetPost(postId);

        // Assert
        redditPost.ShouldNotBeNull();
        redditPost.Count.ShouldBe(2, "a reddit post is made up of two listings: one for the main post and one for the replies");
        redditPost[0].Data.ShouldNotBeNull();
        redditPost[0].Data.Children.ShouldNotBeNull();
        redditPost[0].Data.Children.Count.ShouldBe(1, "there is only a single main post");
        redditPost[0].Data.Children[0].Kind.ShouldBe("t3", "t3 represents the type of main post");
        redditPost[0].Data.Children[0].Data.ShouldNotBeNull();

        var mainPost = redditPost[0].Data.Children[0].Data;
        mainPost.Id.ShouldBe("1kqiwzc");
        mainPost.Author.ShouldBe("melvman1");
        mainPost.CreatedUtc.ShouldBe(DateTime.Parse("2025-05-19T18:18:05"));
        mainPost.SelfText.ShouldBe("I am just about to enter the programming world, and want to become a software engineer. This work ready college in Sweden has a 2 year long .net developer program with internships at real companies. They also have a similar program but with javascript.\n\nI am wondering if this would be a good path if my dream is to become a freelancer and I want to build easy apps / websites for small startups in Sweden/worldwide.\n\nThis is the program:\n\nProgramming C# – 12 weeks\n\nDevelopment against database and database administration – 9 weeks\n\nWeb development with .NET – 12 weeks\n\nAgile development – 6 weeks\n\nCustomer understanding, consulting and reporting – 3 weeks\n\nApprenticeship at companies – 12 weeks\n\nClean code – 6 weeks\n\nApprenticeship at companies – 16 weeks\n\nExam thesis – 4 weeks");
        mainPost.Content.ShouldBe(mainPost.SelfText);
    }
}