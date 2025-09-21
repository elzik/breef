using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RedditPostTransformerTests
{
    private readonly RawRedditPostTransformer _transformer = new();

    [Fact]
    public void Transform_ValidRedditPost_ReturnsExoectedStructure()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RedditListingData
                {
                    Children = new List<RedditChild>
                    {
                        new RedditChild
                        {
                            Kind = "t3",
                            Data = new RedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                Subreddit = "testsubreddit",
                                Score = 100,
                                SelfText = "This is test content",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RedditListingData
                {
                    Children = new List<RedditChild>
                    {
                        new RedditChild
                        {
                            Kind = "t1",
                            Data = new RedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                Score = 50,
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = new RawRedditListing
                                {
                                    Data = new RedditListingData
                                    {
                                        Children = new List<RedditChild>
                                        {
                                            new RedditChild
                                            {
                                                Kind = "t1",
                                                Data = new RedditCommentData
                                                {
                                                    Id = "reply123",
                                                    Author = "replier",
                                                    Body = "This is a reply",
                                                    Score = 25,
                                                    CreatedUtc = new DateTime(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc),
                                                    Replies = new RawRedditListing
                                                    {
                                                        Data = new RedditListingData
                                                        {
                                                            Children = []
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.ShouldNotBeNull();

        // Verify post
        result.Post.Id.ShouldBe("test123");
        result.Post.Title.ShouldBe("Test Post Title");
        result.Post.Author.ShouldBe("testuser");
        result.Post.Subreddit.ShouldBe("testsubreddit");
        result.Post.Score.ShouldBe(100);
        result.Post.Content.ShouldBe("This is test content");
        result.Post.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        // Verify comments
        result.Comments.Count.ShouldBe(1);
        var comment = result.Comments[0];
        comment.Id.ShouldBe("comment123");
        comment.Author.ShouldBe("commenter");
        comment.Content.ShouldBe("This is a comment");
        comment.Score.ShouldBe(50);
        comment.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc));

        // Verify nested replies
        comment.Replies.Count.ShouldBe(1);
        var reply = comment.Replies[0];
        reply.Id.ShouldBe("reply123");
        reply.Author.ShouldBe("replier");
        reply.Content.ShouldBe("This is a reply");
        reply.Score.ShouldBe(25);
        reply.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc));
        reply.Replies.Count.ShouldBe(0);
    }

    [Fact]
    public void Transform_EmptyRedditPost_ThrowsArgumentException()
    {
        // Arrange
        var redditPost = new RawRedditPost();

        // Act & Assert
        Should.Throw<ArgumentException>(() => _transformer.Transform(redditPost))
            .Message.ShouldContain("Reddit post must have at least 2 listings");
    }

    [Fact]
    public void Transform_NoMainPost_ThrowsArgumentException()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Data = new RedditListingData
                {
                    Children = []
                }
            },
            new RawRedditListing
            {
                Data = new RedditListingData
                {
                    Children = []
                }
            }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => _transformer.Transform(redditPost))
            .Message.ShouldContain("Post listing must contain at least one child");
    }

    [Fact]
    public void Transform_PostWithoutTitle_ThrowsInvalidOperationException()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RedditListingData
                {
                    Children = new List<RedditChild>
                    {
                        new RedditChild
                        {
                            Kind = "t3",
                            Data = new RedditCommentData
                            {
                                Id = "test123",
                                Title = null, // No title
                                Author = "testuser",
                                Subreddit = "testsubreddit",
                                Score = 100,
                                SelfText = "This is test content",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RedditListingData
                {
                    Children = []
                }
            }
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => _transformer.Transform(redditPost))
            .Message.ShouldContain("Reddit post must have a title");
    }
}