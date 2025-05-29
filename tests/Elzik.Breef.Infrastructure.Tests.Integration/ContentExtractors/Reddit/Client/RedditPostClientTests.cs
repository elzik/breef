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
        var postId = "1dtr46l";

        // Act
        var redditPost = await client.GetPost(postId);

        // Assert
        redditPost.ShouldNotBeNull();
    }
}