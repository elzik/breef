using Refit;
using Shouldly;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors.Reddit.Client;

public class SubredditClientTests
{
    private static bool IsRunningInGitHubWorkflow => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

    [SkippableFact]
    public async Task GetNewInSubReddit_ValidSubreddit_ReturnsNewInSubreddit()
    {
        // Arrange
        Skip.If(IsRunningInGitHubWorkflow, "Skipped because requests to reddit.com from GitHub workflows are " +
            "always blocked meaning this test case always fails. This must be run locally instead.");
        var client = RestService.For<ISubredditClient>("https://www.reddit.com/");

        // Act
        var newInSubreddit = await client.GetNewInSubreddit("reddit");

        // Assert
        newInSubreddit.ShouldNotBeNull();
        newInSubreddit.Data.ShouldNotBeNull();
        newInSubreddit.Data.Children.ShouldNotBeNull();
        newInSubreddit.Data.Children.Count.ShouldBe(25);
        foreach (var child in newInSubreddit.Data.Children)
        {
            child.Data.ShouldNotBeNull();
            child.Data.Title.ShouldNotBeNullOrEmpty();
            child.Data.Author.ShouldNotBeNullOrEmpty();
            child.Data.SelfText.ShouldNotBeNull();
            child.Data.Url.ShouldNotBeNullOrEmpty();
            child.Data.Id.ShouldNotBeNullOrEmpty();
        }
    }

    [SkippableFact]
    public async Task GetAboutSubreddit_ValidSubreddit_ReturnsAboutSubreddit()
    {
        // Arrange
        Skip.If(IsRunningInGitHubWorkflow, "Skipped because requests to reddit.com from GitHub workflows are " +
            "always blocked meaning this test case always fails. This must be run locally instead.");
        var client = RestService.For<ISubredditClient>("https://www.reddit.com/");

        // Act
        var aboutSubreddit = await client.GetAboutSubreddit("reddit");

        // Assert
        aboutSubreddit.ShouldNotBeNull();
        aboutSubreddit.Data.ShouldNotBeNull();
        aboutSubreddit.Data.PublicDescription.ShouldNotBeNull();
        aboutSubreddit.Data.IconImg.ShouldNotBeNull();
        aboutSubreddit.Data.BannerImg.ShouldNotBeNull();
        aboutSubreddit.Data.BannerBackgroundImage.ShouldNotBeNull();
        aboutSubreddit.Data.MobileBannerImage.ShouldNotBeNull();
        aboutSubreddit.Data.CommunityIcon.ShouldNotBeNull();
    }
}
