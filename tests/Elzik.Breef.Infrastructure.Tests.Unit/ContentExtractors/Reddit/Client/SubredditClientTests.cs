using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class SubredditClientTests
{
    private readonly IRawSubredditClient _mockRawClient;
    private readonly IRawNewInSubredditTransformer _mockTransformer;
    private readonly SubredditClient _client;

    public SubredditClientTests()
    {
        _mockRawClient = Substitute.For<IRawSubredditClient>();
        _mockTransformer = Substitute.For<IRawNewInSubredditTransformer>();
        _client = new SubredditClient(_mockRawClient, _mockTransformer);
    }

    [Fact]
    public async Task GetNewInSubreddit_ValidSubredditName_ReturnsTransformedResult()
    {
        // Arrange
        var subRedditName = "test";
        var rawNewInSubreddit = new RawNewInSubreddit
        {
            Data = new RawListingData
            {
                Children =
                [
                    new RawChild
                    {
                        Data = new RawPostData
                        {
                            Id = "post1",
                            Title = "Test Post"
                        }
                    }
                ]
            }
        };

        var expectedResult = new NewInSubreddit
        {
            Posts =
            [
                new RedditPost
                {
                    Post = new RedditPostContent
                    {
                        Id = "post1",
                        Title = "Test Post",
                        Author = "testuser",
                        Score = 100
                    },
                    Comments = []
                }
            ]
        };

        _mockRawClient.GetNewInSubreddit(subRedditName).Returns(Task.FromResult(rawNewInSubreddit));
        _mockTransformer.Transform(rawNewInSubreddit).Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _client.GetNewInSubreddit(subRedditName);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(1);
        result.Posts[0].Post.Id.ShouldBe("post1");
        result.Posts[0].Post.Title.ShouldBe("Test Post");
    }
}