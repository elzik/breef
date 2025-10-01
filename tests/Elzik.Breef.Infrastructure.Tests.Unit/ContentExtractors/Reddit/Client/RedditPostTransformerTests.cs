using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using Shouldly;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RedditPostTransformerTests
{
    private readonly RawRedditPostTransformer _transformer = new();

    [Fact]
    public void Transform_ValidRedditPost_ReturnsExpectedStructure()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                                        Children = new List<RawRedditChild>
                                        {
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
                Data = new RawRedditListingData
                {
                    Children = []
                }
            },
            new RawRedditListing
            {
                Data = new RawRedditListingData
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
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
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
                Data = new RawRedditListingData
                {
                    Children = []
                }
            }
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => _transformer.Transform(redditPost))
            .Message.ShouldContain("Reddit post must have a title");
    }

    [Fact]
    public void Transform_CommentWithNullReplies_HandlesGracefully()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                                Replies = null // Null replies - should be handled gracefully
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
        result.Comments.Count.ShouldBe(1);
        var comment = result.Comments[0];
        comment.Id.ShouldBe("comment123");
        comment.Replies.ShouldNotBeNull();
        comment.Replies.Count.ShouldBe(0, "null replies should result in empty list");
    }

    [Fact]
    public void Transform_CommentWithEmptyStringReplies_HandlesGracefully()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                                Replies = "" // Empty string replies - Reddit API quirk
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
        result.Comments.Count.ShouldBe(1);
        var comment = result.Comments[0];
        comment.Replies.Count.ShouldBe(0, "empty string should result in empty list");
    }

    [Fact]
    public void Transform_CommentWithJsonElementReplies_HandlesGracefully()
    {
        // Arrange - Create JsonElement for empty string
        var emptyStringJson = JsonSerializer.SerializeToElement("");

        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                                Replies = emptyStringJson // JsonElement with empty string
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
        result.Comments.Count.ShouldBe(1);
        var comment = result.Comments[0];
        comment.Replies.Count.ShouldBe(0, "JsonElement empty string should result in empty list");
    }

    [Fact]
    public void Transform_CommentWithJsonElementNullReplies_HandlesGracefully()
    {
        // Arrange - Create JsonElement for null
        var nullJson = JsonSerializer.SerializeToElement((string?)null);

        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = nullJson // JsonElement with null
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0);
    }

    [Fact]
    public void Transform_CommentWithInvalidJsonElementReplies_HandlesGracefully()
    {
        // Arrange - Create JsonElement for invalid data
        var invalidJson = JsonSerializer.SerializeToElement(123);

        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = invalidJson // JsonElement that can't be deserialized as RawRedditListing
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0, "invalid JsonElement should result in empty list");
    }

    [Fact]
    public void Transform_CommentWithUnknownTypeReplies_HandlesGracefully()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = new { someUnknownProperty = "value" } // Unknown object type
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0, "unknown type should result in empty list");
    }

    [Fact]
    public void Transform_CommentWithRawRedditListingWithNullData_HandlesGracefully()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = new RawRedditListing { Data = null } // RawRedditListing with null Data
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0, "null Data should result in empty list");
    }

    [Fact]
    public void Transform_CommentWithRawRedditListingWithNullChildren_HandlesGracefully()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "This is a comment",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = new RawRedditListing
                                {
                                    Data = new RawRedditListingData { Children = null }
                                } // RawRedditListing with null Children
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Replies.Count.ShouldBe(0, "null Children should result in empty list");
    }

    [Fact]
    public void Transform_CommentsWithDifferentKinds_OnlyProcessesT1Comments()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
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
                            Kind = "t3", // Post - should be ignored in comments section
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
                            Kind = "more", // More comments indicator - should be ignored
                            Data = new RawRedditCommentData
                            {
                                Id = "more789",
                                Author = "system",
                                Body = "Load more comments",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 40, 0, DateTimeKind.Utc),
                                Replies = null
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1, "only the t1 comment should be processed");
        result.Comments[0].Id.ShouldBe("comment123");
        result.Comments[0].Author.ShouldBe("commenter");
    }

    [Fact]
    public void Transform_PostWithNullFields_HandlesNullsGracefully()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = null, // Null ID
                                Title = "Test Post Title",
                                Author = null, // Null Author
                                Subreddit = null, // Null Subreddit
                                Score = 100,
                                SelfText = null, // Null Content
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
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
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.Id.ShouldBe(string.Empty, "null ID becomes empty string");
        result.Post.Title.ShouldBe("Test Post Title");
        result.Post.Author.ShouldBe(string.Empty, "null Author becomes empty string");
        result.Post.Subreddit.ShouldBe(string.Empty, "null Subreddit becomes empty string");
        result.Post.Content.ShouldBe(string.Empty, "null Content becomes empty string");
        result.Post.Score.ShouldBe(100);
    }

    [Fact]
    public void Transform_CommentWithNullFields_HandlesNullsGracefully()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Test Post Title",
                                Author = "testuser",
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                            }
                        }
                    }
                }
            },
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = new List<RawRedditChild>
                    {
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = null, // Null ID
                                Author = null, // Null Author
                                Body = null, // Null Body
                                Score = 50,
                                CreatedUtc = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                                Replies = null
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1);
        var comment = result.Comments[0];
        comment.Id.ShouldBe(string.Empty, "null ID becomes empty string");
        comment.Author.ShouldBe(string.Empty, "null Author becomes empty string");
        comment.Content.ShouldBe(string.Empty, "null Content becomes empty string");
        comment.Score.ShouldBe(50);
    }

    [Fact]
    public void Transform_NullRawRedditPost_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _transformer.Transform(null!))
            .ParamName.ShouldBe("rawRedditPost");
    }
}