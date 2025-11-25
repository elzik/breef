using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RawNewInSubredditTransformerTests
{
    private readonly IRedditPostClient _redditPostClient;
    private readonly RawNewInSubredditTransformer _transformer;

    public RawNewInSubredditTransformerTests()
    {
        _redditPostClient = Substitute.For<IRedditPostClient>();
        _transformer = new RawNewInSubredditTransformer(_redditPostClient);
    }

    [Fact]
    public async Task Transform_ValidRawNewInSubreddit_ReturnsExpectedStructure()
    {
        // Arrange
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
                            Title = "Test Post 1",
                            Author = "author1"
                        }
                    },
                    new RawChild
                    {
                        Data = new RawPostData
                        {
                            Id = "post2",
                            Title = "Test Post 2",
                            Author = "author2"
                        }
                    }
                ]
            }
        };

        var redditPost1 = new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = "post1",
                Title = "Test Post 1",
                Author = "author1",
                Score = 100,
                Content = "Content 1",
                CreatedUtc = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                PostUrl = "https://reddit.com/r/testsubreddit/comments/post1"
            },
            Comments = []
        };

        var redditPost2 = new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = "post2",
                Title = "Test Post 2",
                Author = "author2",
                Score = 200,
                Content = "Content 2",
                CreatedUtc = new DateTime(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc),
                PostUrl = "https://reddit.com/r/testsubreddit/comments/post2"
            },
            Comments = []
        };

        _redditPostClient.GetPost("post1").Returns(Task.FromResult(redditPost1));
        _redditPostClient.GetPost("post2").Returns(Task.FromResult(redditPost2));

        // Act
        var result = await _transformer.Transform(rawNewInSubreddit);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(2);

        var firstPost = result.Posts[0];
        firstPost.Post.Id.ShouldBe("post1");
        firstPost.Post.Title.ShouldBe("Test Post 1");
        firstPost.Post.Author.ShouldBe("author1");
        firstPost.Post.Score.ShouldBe(100);
        firstPost.Post.Content.ShouldBe("Content 1");
        firstPost.Post.PostUrl.ShouldBe("https://reddit.com/r/testsubreddit/comments/post1");

        var secondPost = result.Posts[1];
        secondPost.Post.Id.ShouldBe("post2");
        secondPost.Post.Title.ShouldBe("Test Post 2");
        secondPost.Post.Author.ShouldBe("author2");
        secondPost.Post.Score.ShouldBe(200);
        secondPost.Post.Content.ShouldBe("Content 2");
        secondPost.Post.PostUrl.ShouldBe("https://reddit.com/r/testsubreddit/comments/post2");
    }

    [Fact]
    public async Task Transform_EmptyChildren_ReturnsEmptyNewInSubreddit()
    {
        // Arrange
        var rawNewInSubreddit = new RawNewInSubreddit
        {
            Data = new RawListingData
            {
                Children = []
            }
        };

        // Act
        var result = await _transformer.Transform(rawNewInSubreddit);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Transform_NullChildren_ReturnsEmptyNewInSubreddit()
    {
        // Arrange
        var rawNewInSubreddit = new RawNewInSubreddit
        {
            Data = new RawListingData
            {
                Children = null
            }
        };

        // Act
        var result = await _transformer.Transform(rawNewInSubreddit);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Transform_NullData_ReturnsEmptyNewInSubreddit()
    {
        // Arrange
        var rawNewInSubreddit = new RawNewInSubreddit
        {
            Data = null
        };

        // Act
        var result = await _transformer.Transform(rawNewInSubreddit);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Transform_ChildrenWithNullData_SkipsNullDataChildren()
    {
        // Arrange
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
                            Title = "Valid Post"
                        }
                    },
                    new RawChild
                    {
                        Data = null
                    },
                    new RawChild
                    {
                        Data = new RawPostData
                        {
                            Id = "post2",
                            Title = "Another Valid Post"
                        }
                    }
                ]
            }
        };

        var redditPost1 = new RedditPost
        {
            Post = new RedditPostContent { Id = "post1", Title = "Valid Post" },
            Comments = []
        };

        var redditPost2 = new RedditPost
        {
            Post = new RedditPostContent { Id = "post2", Title = "Another Valid Post" },
            Comments = []
        };

        _redditPostClient.GetPost("post1").Returns(Task.FromResult(redditPost1));
        _redditPostClient.GetPost("post2").Returns(Task.FromResult(redditPost2));

        // Act
        var result = await _transformer.Transform(rawNewInSubreddit);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(2);
        result.Posts[0].Post.Id.ShouldBe("post1");
        result.Posts[1].Post.Id.ShouldBe("post2");
    }

    [Fact]
    public async Task Transform_ChildrenWithNullIds_SkipsNullIdChildren()
    {
        // Arrange
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
                            Title = "Valid Post"
                        }
                    },
                    new RawChild
                    {
                        Data = new RawPostData
                        {
                            Id = null, // This should be skipped
                            Title = "Post with null ID"
                        }
                    },
                    new RawChild
                    {
                        Data = new RawPostData
                        {
                            Id = "post2",
                            Title = "Another Valid Post"
                        }
                    }
                ]
            }
        };

        var redditPost1 = new RedditPost
        {
            Post = new RedditPostContent { Id = "post1", Title = "Valid Post" },
            Comments = []
        };

        var redditPost2 = new RedditPost
        {
            Post = new RedditPostContent { Id = "post2", Title = "Another Valid Post" },
            Comments = []
        };

        _redditPostClient.GetPost("post1").Returns(Task.FromResult(redditPost1));
        _redditPostClient.GetPost("post2").Returns(Task.FromResult(redditPost2));

        // Act
        var result = await _transformer.Transform(rawNewInSubreddit);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(2);
        result.Posts[0].Post.Id.ShouldBe("post1");
        result.Posts[1].Post.Id.ShouldBe("post2");
    }

    [Fact]
    public async Task Transform_SinglePost_ReturnsNewInSubredditWithOnePost()
    {
        // Arrange
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
                            Id = "single_post",
                            Title = "Single Test Post",
                            Author = "single_author",
                            SelfText = "This is a single post",
                            Url = "https://reddit.com/r/test/single_post"
                        }
                    }
                ]
            }
        };

        var redditPost = new RedditPost
        {
            Post = new RedditPostContent
            {
                Id = "single_post",
                Title = "Single Test Post",
                Author = "single_author",
                Content = "This is a single post",
                Score = 42,
                Subreddit = "test",
                CreatedUtc = new DateTime(2025, 1, 1, 14, 0, 0, DateTimeKind.Utc),
                ImageUrl = "https://example.com/image.jpg",
                PostUrl = "https://reddit.com/r/test/single_post"
            },
            Comments =
            [
                new RedditComment
                {
                    Id = "comment1",
                    Author = "commenter",
                    Content = "Great post!",
                    Score = 5,
                    CreatedUtc = new DateTime(2025, 1, 1, 14, 30, 0, DateTimeKind.Utc),
                    Replies = []
                }
            ]
        };

        _redditPostClient.GetPost("single_post").Returns(Task.FromResult(redditPost));

        // Act
        var result = await _transformer.Transform(rawNewInSubreddit);

        // Assert
        result.ShouldNotBeNull();
        result.Posts.Count.ShouldBe(1);

        var post = result.Posts[0];
        post.Post.Id.ShouldBe("single_post");
        post.Post.Title.ShouldBe("Single Test Post");
        post.Post.Author.ShouldBe("single_author");
        post.Post.Content.ShouldBe("This is a single post");
        post.Post.Score.ShouldBe(42);
        post.Post.Subreddit.ShouldBe("test");
        post.Post.ImageUrl.ShouldBe("https://example.com/image.jpg");
        post.Post.PostUrl.ShouldNotBeNullOrEmpty();
        post.Comments.Count.ShouldBe(1);
        post.Comments[0].Content.ShouldBe("Great post!");
    }

    [Fact]
    public async Task Transform_NullRawNewInSubreddit_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentNullException>(() => _transformer.Transform(null!));
        exception.ParamName.ShouldBe("rawNewInSubreddit");
    }

    [Fact]
    public async Task Transform_ConcurrentPostFetching_CallsClientConcurrently()
    {
        // Arrange
        var rawNewInSubreddit = new RawNewInSubreddit
        {
            Data = new RawListingData
            {
                Children =
                [
                    new RawChild { Data = new RawPostData { Id = "post1" } },
                    new RawChild { Data = new RawPostData { Id = "post2" } },
                    new RawChild { Data = new RawPostData { Id = "post3" } }
                ]
            }
        };

        var tcs1 = new TaskCompletionSource<RedditPost>();
        var tcs2 = new TaskCompletionSource<RedditPost>();
        var tcs3 = new TaskCompletionSource<RedditPost>();

        _redditPostClient.GetPost("post1").Returns(tcs1.Task);
        _redditPostClient.GetPost("post2").Returns(tcs2.Task);
        _redditPostClient.GetPost("post3").Returns(tcs3.Task);

        // Act
        var transformTask = _transformer.Transform(rawNewInSubreddit);

        // Complete the tasks
        tcs1.SetResult(new RedditPost { Post = new RedditPostContent { Id = "post1" }, Comments = [] });
        tcs2.SetResult(new RedditPost { Post = new RedditPostContent { Id = "post2" }, Comments = [] });
        tcs3.SetResult(new RedditPost { Post = new RedditPostContent { Id = "post3" }, Comments = [] });

        var result = await transformTask;

        // Assert
        result.Posts.Count.ShouldBe(3);
        result.Posts.Select(p => p.Post.Id).ShouldBe(["post1", "post2", "post3"]);
    }
}