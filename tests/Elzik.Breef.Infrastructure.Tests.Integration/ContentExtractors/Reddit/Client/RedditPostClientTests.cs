using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using Refit;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors.Reddit.Client;

public class RedditPostClientTests
{
    private static bool IsRunningInGitHubWorkflow => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

    [SkippableFact]
    public async Task GetPost_ValidPostId_ReturnsExpectedRedditPost()
    {
        // Arrange
        Skip.If(IsRunningInGitHubWorkflow, "Skipped because requests to reddit.com from GitHub workflows are " +
            "always blocked meaning this test case always fails. This must be run locally instead.");

        var rawRedditClient = RestService.For<IRawRedditPostClient>("https://www.reddit.com/");
        var transformer = new RawRedditPostTransformer();
        var redditClient = new RedditPostClient(rawRedditClient, transformer);
        var postId = "1kqiwzc"; // https://www.reddit.com/r/learnprogramming/comments/1kqiwzc

        // Act
        var redditPost = await redditClient.GetPost(postId);

        // Assert
        redditPost.ShouldNotBeNull();

        // Verify post structure
        redditPost.Post.ShouldNotBeNull();
        redditPost.Post.Id.ShouldBe("1kqiwzc");
        redditPost.Post.Author.ShouldBeOneOf("melvman1", "[deleted]");
        redditPost.Post.Title.ShouldNotBeNullOrWhiteSpace();
        redditPost.Post.Content.ShouldNotBeNullOrWhiteSpace();

        // Verify comments structure
        redditPost.Comments.ShouldNotBeNull();
        redditPost.Comments.Count.ShouldBe(5);

        // Find and verify specific comments by ID
        var firstComment = redditPost.Comments.Single(c => c.Id == "mt7aaf6");
        firstComment.Author.ShouldBeOneOf("CodeRadDesign", "[deleted]");
        firstComment.Content.ShouldBeOneOf(
            "not really.\n\nas someone who's been freelance on and off for 30 years, you're looking for a more " +
            "rounded skill set.  \n\nyou're not going to compete with 'people from third world countries' like the " +
            "other poster mentioned; you just can't. so you have to ask yourself, what do people in my area actually " +
            "need. \n\nif the answer is (and it probably is) websites for their local businesses, then you want a mix " +
            "of graphic art, html/css/js, a frontend tech like react or vue, and a backend tech. that could be C#.net" +
            ", that could by python, lots of options.\n\nC# is definitely in demand, but not so much in freelance. " +
            "for the most part a C#.net core specialist is going to be part of a team, at a company, and you'll defo " +
            "want that college paper for that.  if you're only planning on freelance, you can realistically just self " +
            "learn. if you don't think you can handle the unstructuredness of self-learning..... you're going to hate " +
            "freelancing. \n\notherwise looks like a fine program, i would likely favor taking something like that " +
            "and planning on getting a Real Job though haha.\n\n*regarding your last point on your other comment \"" +
            "c# looks easy to learn\" is not really a valid criteria. your first language is going to be the hardest" +
            ", your second language will be ten times easier. c# is a good foundational language tho, i'd recommend " +
            "it over python because it teaches a lot of good habits early.",
            "[deleted]"
        );

        var secondComment = redditPost.Comments.Single(c => c.Id == "mt7lqgx");
        secondComment.Author.ShouldBeOneOf("No_Researcher_7875", "[deleted]");

        var thirdComment = redditPost.Comments.Single(c => c.Id == "mt606l6");
        thirdComment.Author.ShouldBeOneOf("[deleted]");

        // Verify nested replies
        thirdComment.Replies.ShouldNotBeNull();
        thirdComment.Replies.Count.ShouldBe(1);
        var nestedReply = thirdComment.Replies.Single(r => r.Id == "mt60jnv");
        nestedReply.Author.ShouldBeOneOf("melvman1", "[deleted]");

        var fourthComment = redditPost.Comments.Single(c => c.Id == "mt83c0a");
        fourthComment.Author.ShouldBeOneOf("goqsane", "[deleted]");

        var fifthComment = redditPost.Comments.Single(c => c.Id == "mt9gc9x");
        fifthComment.Author.ShouldBeOneOf("ToThePillory", "[deleted]");
    }
}