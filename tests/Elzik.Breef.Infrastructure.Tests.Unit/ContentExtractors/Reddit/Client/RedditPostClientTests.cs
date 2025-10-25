using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using NSubstitute;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RedditPostClientTests
{
    private readonly IRawRedditPostClient _mockRawClient;
    private readonly IRawRedditPostTransformer _mockTransformer;
    private readonly RedditPostClient _client;

    public RedditPostClientTests()
    {
        _mockRawClient = Substitute.For<IRawRedditPostClient>();
        _mockTransformer = Substitute.For<IRawRedditPostTransformer>();
        _client = new RedditPostClient(_mockRawClient, _mockTransformer);
    }

    [Fact]
    public async Task GetPost_ValidRedditPost_ReturnsTransformedPost()
    {
        // Arrange
        var postId = "1kqiwzc";
        var rawRedditPost = CreateValidRawRedditPost();
        var expectedResult = CreateExpectedTransformedResult();

        _mockRawClient.GetPost(postId).Returns(Task.FromResult(rawRedditPost));
        _mockTransformer.Transform(rawRedditPost).Returns(expectedResult);

        // Act
        var result = await _client.GetPost(postId);

        // Assert
        result.ShouldNotBeNull();

        result.Post.ShouldNotBeNull();
        result.Post.Id.ShouldBe("test123");
        result.Post.Title.ShouldBe("Test Post Title");
        result.Post.Author.ShouldBe("testuser");
        result.Post.Subreddit.ShouldBe("testsubreddit");
        result.Post.Score.ShouldBe(100);
        result.Post.Content.ShouldBe("This is test content");
        result.Post.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));


        result.Comments.ShouldNotBeNull();
        result.Comments.Count.ShouldBe(1);

        var comment = result.Comments[0];
        comment.Id.ShouldBe("comment123");
        comment.Author.ShouldBe("commenter");
        comment.Content.ShouldBe("This is a comment");
        comment.Score.ShouldBe(50);
        comment.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc));


        comment.Replies.Count.ShouldBe(1);
        var reply = comment.Replies[0];
        reply.Id.ShouldBe("reply123");
        reply.Author.ShouldBe("replier");
        reply.Content.ShouldBe("This is a reply");
        reply.Score.ShouldBe(25);
        reply.Replies.Count.ShouldBe(0);


        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    [Fact]
    public async Task GetPost_PostWithEmptyStringReplies_HandlesGracefully()
    {
        // Arrange
        var postId = "test456";
        var rawRedditPost = CreateRawRedditPostWithEmptyStringReplies();
        var expectedResult = CreateExpectedResultWithEmptyReplies();

        _mockRawClient.GetPost(postId).Returns(Task.FromResult(rawRedditPost));
        _mockTransformer.Transform(rawRedditPost).Returns(expectedResult);

        // Act
        var result = await _client.GetPost(postId);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0, "empty string replies should result in empty list");

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    [Fact]
    public async Task GetPost_PostWithNullReplies_HandlesGracefully()
    {
        // Arrange
        var postId = "test789";
        var rawRedditPost = CreateRawRedditPostWithNullReplies();
        var expectedResult = CreateExpectedResultWithEmptyReplies();

        _mockRawClient.GetPost(postId).Returns(Task.FromResult(rawRedditPost));
        _mockTransformer.Transform(rawRedditPost).Returns(expectedResult);

        // Act
        var result = await _client.GetPost(postId);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0, "null replies should result in empty list");

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    [Fact]
    public async Task GetPost_PostWithJsonElementReplies_HandlesGracefully()
    {
        // Arrange
        var postId = "testjson";
        var rawRedditPost = CreateRawRedditPostWithJsonElementReplies();
        var expectedResult = CreateExpectedResultWithEmptyReplies();

        _mockRawClient.GetPost(postId).Returns(Task.FromResult(rawRedditPost));
        _mockTransformer.Transform(rawRedditPost).Returns(expectedResult);

        // Act
        var result = await _client.GetPost(postId);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0, "JsonElement empty string should result in empty list");

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    [Fact]
    public async Task GetPost_PostWithMixedCommentTypes_OnlyProcessesComments()
    {
        // Arrange
        var postId = "testmixed";
        var rawRedditPost = CreateRawRedditPostWithMixedCommentTypes();
        var expectedResult = CreateExpectedResultWithSingleComment();

        _mockRawClient.GetPost(postId).Returns(Task.FromResult(rawRedditPost));
        _mockTransformer.Transform(rawRedditPost).Returns(expectedResult);

        // Act
        var result = await _client.GetPost(postId);

        // Assert
        result.Comments.Count.ShouldBe(1, "only t1 (comment) types should be processed");
        result.Comments[0].Id.ShouldBe("comment123");
        result.Comments[0].Author.ShouldBe("commenter");

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    [Fact]
    public async Task GetPost_PostWithNullFields_HandlesNullsGracefully()
    {
        // Arrange
        var postId = "testnulls";
        var rawRedditPost = CreateRawRedditPostWithNullFields();
        var expectedResult = CreateExpectedResultWithNullFields();

        _mockRawClient.GetPost(postId).Returns(Task.FromResult(rawRedditPost));
        _mockTransformer.Transform(rawRedditPost).Returns(expectedResult);

        // Act
        var result = await _client.GetPost(postId);

        // Assert
        result.Post.Id.ShouldBe(string.Empty, "null ID should become empty string");
        result.Post.Title.ShouldBe("Test Post Title");
        result.Post.Author.ShouldBe(string.Empty, "null Author should become empty string");
        result.Post.Content.ShouldBe(string.Empty, "null Content should become empty string");

        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Id.ShouldBe(string.Empty, "null comment ID should become empty string");
        result.Comments[0].Author.ShouldBe(string.Empty, "null comment Author should become empty string");
        result.Comments[0].Content.ShouldBe(string.Empty, "null comment Content should become empty string");

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    [Fact]
    public async Task GetPost_PostWithoutTitle_ThrowsInvalidOperationException()
    {
        // Arrange
        var postId = "notitle";
        var rawRedditPost = CreateRawRedditPostWithoutTitle();

        _mockRawClient.GetPost(postId).Returns(rawRedditPost);
        _mockTransformer.Transform(rawRedditPost).Returns<RedditPost>(_ => throw new InvalidOperationException("Reddit post must have a title"));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _client.GetPost(postId));

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    [Fact]
    public async Task GetPost_EmptyRawPost_ThrowsArgumentException()
    {
        // Arrange
        var postId = "empty";
        var emptyRawPost = new RawRedditPost();
        var emptyRawPostParamName = nameof(emptyRawPost);

        _mockRawClient.GetPost(postId).Returns(emptyRawPost);
        _mockTransformer.Transform(emptyRawPost).Returns<RedditPost>(_ => 
            throw new ArgumentException("Reddit post must have at least 2 listings (post and comments)", emptyRawPostParamName));

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => _client.GetPost(postId));

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(emptyRawPost);
    }

    [Fact]
    public async Task GetPost_PostWithNoChildren_ThrowsArgumentException()
    {
        // Arrange
        var postId = "nochildren";
        var rawRedditPost = CreateRawRedditPostWithNoChildren();
        var rawRedditPostParamName = nameof(rawRedditPost);

        _mockRawClient.GetPost(postId).Returns(Task.FromResult(rawRedditPost));
        _mockTransformer.Transform(rawRedditPost).Returns<RedditPost>(_ => 
            throw new ArgumentException("Post listing must contain at least one child", rawRedditPostParamName));

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => _client.GetPost(postId));

        _ = _mockRawClient.Received(1).GetPost(postId);
        _ = _mockTransformer.Received(1).Transform(rawRedditPost);
    }

    #region Test Data Factory Methods

    private static RedditPost CreateExpectedTransformedResult()
    {
        return new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = "test123",
                Title = "Test Post Title",
                Author = "testuser",
                Subreddit = "testsubreddit",
                Score = 100,
                Content = "This is test content",
                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            },
            Comments =
            [
                new() {
                    Id = "comment123",
                    Author = "commenter",
                    Content = "This is a comment",
                    Score = 50,
                    CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                    Replies =
                    [
                        new() {
                            Id = "reply123",
                            Author = "replier",
                            Content = "This is a reply",
                            Score = 25,
                            CreatedUtc = new DateTime(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc),
                            Replies = []
                        }
                    ]
                }
            ]
        };
    }

    private static RedditPost CreateExpectedResultWithEmptyReplies()
    {
        return new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = "test456",
                Title = "Test Post Title",
                Author = "testuser",
                Subreddit = "testsubreddit",
                Score = 0,
                Content = string.Empty,
                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            },
            Comments =
            [
                new() {
                    Id = "comment456",
                    Author = "commenter",
                    Content = "This is a comment",
                    Score = 0,
                    CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                    Replies = []
                }
            ]
        };
    }

    private static RedditPost CreateExpectedResultWithSingleComment()
    {
        return new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = "testmixed",
                Title = "Test Post Title",
                Author = "testuser",
                Subreddit = "testsubreddit",
                Score = 0,
                Content = string.Empty,
                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            },
            Comments =
            [
                new() {
                    Id = "comment123",
                    Author = "commenter",
                    Content = "This is a comment",
                    Score = 0,
                    CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                    Replies = []
                }
            ]
        };
    }

    private static RedditPost CreateExpectedResultWithNullFields()
    {
        return new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = string.Empty,
                Title = "Test Post Title",
                Author = string.Empty,
                Subreddit = string.Empty,
                Score = 0,
                Content = string.Empty,
                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            },
            Comments =
            [
                new() {
                    Id = string.Empty,
                    Author = string.Empty,
                    Content = string.Empty,
                    Score = 0,
                    CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                    Replies = []
                }
            ]
        };
    }

    private static RawRedditPost CreateValidRawRedditPost()
    {
        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
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
                    ]
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                Score = 50,
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = new RawRedditListing
                                {
                                    Data = new RawRedditListingData
                                    {
                                        Children =
                                        [
                                            new RawRedditChild
                                            {
                                                Kind = "t1",
                                                Data = new RawRedditCommentData
                                                {
                                                    Id = "reply123",
                                                    Author = "replier",
                                                    Body = "This is a reply",
                                                    Score = 25,
                                                    CreatedUtc = new DateTime(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc),
                                                    Replies = new RawRedditListing
                                                    {
                                                        Data = new RawRedditListingData
                                                        {
                                                            Children = []
                                                        }
                                                    }
                                                }
                                            }
                                        ]
                                    }
                                }
                            }
                        }
                    ]
                }
            }
        ];
    }

    private static RawRedditPost CreateRawRedditPostWithEmptyStringReplies()
    {
        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test456",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    ]
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment456",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = "" // Empty string - Reddit API quirk
                            }
                        }
                    ]
                }
            }
        ];
    }

    private static RawRedditPost CreateRawRedditPostWithNullReplies()
    {
        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test789",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    ]
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment789",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = null // Null replies
                            }
                        }
                    ]
                }
            }
        ];
    }

    private static RawRedditPost CreateRawRedditPostWithJsonElementReplies()
    {
        var emptyStringJson = JsonSerializer.SerializeToElement("");

        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "testjson",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    ]
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "commentjson",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = emptyStringJson // JsonElement with empty string
                            }
                        }
                    ]
                }
            }
        ];
    }

    private static RawRedditPost CreateRawRedditPostWithMixedCommentTypes()
    {
        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "testmixed",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    ]
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t1", // Comment - should be processed
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = null
                            }
                        },
                        new RawRedditChild
                        {
                            Kind = "t3", // Post - should be ignored
                            Data = new RawRedditCommentData
                            {
                                Id = "post456",
                                Author = "poster",
                                Body = "This should be ignored",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 35, 0, DateTimeKind.Utc),
                                Replies = null
                            }
                        },
                        new RawRedditChild
                        {
                            Kind = "more", // More comments - should be ignored
                            Data = new RawRedditCommentData
                            {
                                Id = "more789",
                                Author = "system",
                                Body = "Load more comments",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 40, 0, DateTimeKind.Utc),
                                Replies = null
                            }
                        }
                    ]
                }
            }
        ];
    }

    private static RawRedditPost CreateRawRedditPostWithNullFields()
    {
        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = null, // Null ID
                                Title = "Test Post Title",
                                Author = null, // Null Author
                                Subreddit = null, // Null Subreddit
                                SelfText = null, // Null Content
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    ]
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = null, // Null ID
                                Author = null, // Null Author
                                Body = null, // Null Body
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = null
                            }
                        }
                    ]
                }
            }
        ];
    }

    private static RawRedditPost CreateRawRedditPostWithoutTitle()
    {
        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "notitle",
                                Title = null, // No title - should throw
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    ]
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = []
                }
            }
        ];
    }

    private static RawRedditPost CreateRawRedditPostWithNoChildren()
    {
        return
        [
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = [] // No children - should throw
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = []
                }
            }
        ];
    }

    #endregion
}