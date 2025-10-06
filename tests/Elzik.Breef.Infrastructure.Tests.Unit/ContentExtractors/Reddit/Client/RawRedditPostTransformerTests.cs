using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RawRedditPostTransformerTests
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

        result.Post.Id.ShouldBe("test123");
        result.Post.Title.ShouldBe("Test Post Title");
        result.Post.Author.ShouldBe("testuser");
        result.Post.Subreddit.ShouldBe("testsubreddit");
        result.Post.Score.ShouldBe(100);
        result.Post.Content.ShouldBe("This is test content");
        result.Post.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        
        result.Post.ImageUrl.ShouldBeNull();

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
        reply.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc));
        reply.Replies.Count.ShouldBe(0);
    }

    [Fact]
    public void Transform_PostWithDirectImageUrl_ExtractsImageCorrectly()
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
                                Title = "Image Post",
                                Author = "testuser",
                                Url = "https://i.redd.it/example.jpg",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    }
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://i.redd.it/example.jpg");
    }

    [Fact]
    public void Transform_PostWithPreviewImage_ExtractsImageCorrectly()
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
                                Title = "Preview Image Post",
                                Author = "testuser",
                                Preview = new RawRedditPreview
                                {
                                    Enabled = true,
                                    Images = new List<RawRedditPreviewImage>
                                    {
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/example.jpg",
                                                Width = 800,
                                                Height = 600
                                            }
                                        }
                                    }
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    }
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://preview.redd.it/example.jpg");
    }

    [Fact]
    public void Transform_PostWithGallery_ExtractsFirstImageCorrectly()
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
                                Title = "Gallery Post",
                                Author = "testuser",
                                IsGallery = true,
                                GalleryData = new RawRedditGalleryData
                                {
                                    Items = new List<RawRedditGalleryItem>
                                    {
                                        new RawRedditGalleryItem { MediaId = "img1" },
                                        new RawRedditGalleryItem { MediaId = "img2" }
                                    }
                                },
                                MediaMetadata = new Dictionary<string, RawRedditMediaMetadata>
                                {
                                    ["img1"] = new RawRedditMediaMetadata 
                                    { 
                                        Status = "valid",
                                        Source = new RawRedditImageSource 
                                        { 
                                            Url = "https://i.redd.it/gallery1.jpg",
                                            Width = 1000,
                                            Height = 800
                                        }
                                    },
                                    ["img2"] = new RawRedditMediaMetadata 
                                    { 
                                        Status = "valid",
                                        Source = new RawRedditImageSource 
                                        { 
                                            Url = "https://i.redd.it/gallery2.jpg",
                                            Width = 800,
                                            Height = 600
                                        }
                                    }
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    }
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://i.redd.it/gallery1.jpg");
    }

    [Fact]
    public void Transform_PostWithThumbnailOnly_ExtractsThumbnailCorrectly()
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
                                Title = "Thumbnail Post",
                                Author = "testuser",
                                Thumbnail = "https://b.thumbs.redditmedia.com/thumb.jpg",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    }
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://b.thumbs.redditmedia.com/thumb.jpg");
    }

    [Fact]
    public void Transform_PostWithMultipleImageSources_PrioritizesCorrectly()
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
                                Title = "Multi-source Image Post",
                                Author = "testuser",
                                Url = "https://i.redd.it/direct.jpg",
                                Thumbnail = "https://b.thumbs.redditmedia.com/thumb.jpg",
                                Preview = new RawRedditPreview
                                {
                                    Enabled = true,
                                    Images = new List<RawRedditPreviewImage>
                                    {
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/preview.jpg",
                                                Width = 800,
                                                Height = 600
                                            }
                                        }
                                    }
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    }
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://preview.redd.it/preview.jpg");
    }

    [Fact]
    public void Transform_PostWithInvalidThumbnails_IgnoresInvalidThumbnails()
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
                                Title = "Invalid Thumbnail Post",
                                Author = "testuser",
                                Thumbnail = "self", // Should be ignored
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    }
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBeNull();
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
    public void Transform_NullRawRedditPost_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _transformer.Transform(null!))
            .ParamName.ShouldBe("rawRedditPost");
    }
}