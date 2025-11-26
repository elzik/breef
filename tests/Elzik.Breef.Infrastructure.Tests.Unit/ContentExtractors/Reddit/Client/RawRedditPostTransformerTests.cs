using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit.Client;

public class RawRedditPostTransformerTests
{
    private readonly RawRedditPostTransformer _transformer;

    public RawRedditPostTransformerTests()
    {
        var options = Options.Create(new RedditOptions()
        {
            DefaultBaseAddress = "https://www.test-reddit.com"
        });
        _transformer = new RawRedditPostTransformer(options);
    }

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
        comment.PostUrl.ShouldBe("https://www.test-reddit.com/r/testsubreddit/comments/test123/comment/comment123/");

        comment.Replies.Count.ShouldBe(1);
        var reply = comment.Replies[0];
        reply.Id.ShouldBe("reply123");
        reply.Author.ShouldBe("replier");
        reply.Content.ShouldBe("This is a reply");
        reply.Score.ShouldBe(25);
        reply.CreatedUtc.ShouldBe(new DateTime(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc));
        reply.PostUrl.ShouldBe("https://www.test-reddit.com/r/testsubreddit/comments/test123/comment/reply123/");
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
                    Children =
                    [
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
                    ]
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
                    Children =
                    [
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
                                    Images =
                                    [
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/example.jpg",
                                                Width = 800,
                                                Height = 600
                                            }
                                        }
                                    ]
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
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
                    Children =
                    [
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
                                    Items =
                                    [
                                        new RawRedditGalleryItem { MediaId = "img1" },
                                        new RawRedditGalleryItem { MediaId = "img2" }
                                    ]
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
                    ]
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
                    Children =
                    [
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
                    ]
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
                    Children =
                    [
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
                                    Images =
                                    [
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/preview.jpg",
                                                Width = 800,
                                                Height = 600
                                            }
                                        }
                                    ]
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
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
                    Children =
                    [
                        new RawRedditChild
                        {
                            Kind = "t3",
                            Data = new RawRedditCommentData
                            {
                                Id = "test123",
                                Title = "Invalid Thumbnail Post",
                                Author = "testuser",
                                Thumbnail = "self",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
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


    [Fact]
    public void Instantiated_NullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new RawRedditPostTransformer(null!))
            .ParamName.ShouldBe("options");
    }

    [Fact]
    public void Instantiated_NullOptionsValue_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create<RedditOptions>(null!);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => new RawRedditPostTransformer(options))
            .Message.ShouldContain("RedditOptions configuration is missing or not bound");
    }

    [Fact]
    public void Transform_PostListingWithNoChildren_ThrowsArgumentException()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
            new RawRedditListing
            {
                Kind = "Listing",
                Data = new RawRedditListingData
                {
                    Children = []
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => _transformer.Transform(redditPost))
            .Message.ShouldContain("Post listing must contain at least one child");
    }

    [Fact]
    public void Transform_PostWithNullTitle_ThrowsInvalidOperationException()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = null,
                                Author = "testuser",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => _transformer.Transform(redditPost))
            .Message.ShouldContain("Reddit post must have a title");
    }

    [Fact]
    public void Transform_InvalidRedditOptionsBaseAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new RedditOptions
        {
            DefaultBaseAddress = "not-a-valid-uri"
        });
        var transformer = new RawRedditPostTransformer(options);

        var redditPost = new RawRedditPost
        {
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
                                Title = "Test Post",
                                Author = "testuser",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => transformer.Transform(redditPost))
            .Message.ShouldContain("RedditOptions.DefaultBaseAddress is not a valid absolute URI");
    }

    [Fact]
    public void Transform_PostWithHtmlEncodedGalleryUrl_DecodesUrlCorrectly()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Gallery Post",
                                Author = "testuser",
                                IsGallery = true,
                                GalleryData = new RawRedditGalleryData
                                {
                                    Items = [new RawRedditGalleryItem { MediaId = "img1" }]
                                },
                                MediaMetadata = new Dictionary<string, RawRedditMediaMetadata>
                                {
                                    ["img1"] = new RawRedditMediaMetadata
                                    {
                                        Status = "valid",
                                        Source = new RawRedditImageSource
                                        {
                                            Url = "https://i.redd.it/gallery&amp;test.jpg",
                                            Width = 1000,
                                            Height = 800
                                        }
                                    }
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://i.redd.it/gallery&test.jpg");
    }

    [Fact]
    public void Transform_PostWithHtmlEncodedPreviewUrl_DecodesUrlCorrectly()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Preview Post",
                                Author = "testuser",
                                Preview = new RawRedditPreview
                                {
                                    Enabled = true,
                                    Images =
                                    [
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/test&amp;image.jpg",
                                                Width = 800,
                                                Height = 600
                                            }
                                        }
                                    ]
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://preview.redd.it/test&image.jpg");
    }

    [Fact]
    public void Transform_PostWithUrlOverriddenByDest_UsesOverriddenUrl()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Overridden URL Post",
                                Author = "testuser",
                                Url = "https://i.redd.it/original.jpg",
                                UrlOverriddenByDest = "https://i.redd.it/overridden.jpg",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://i.redd.it/overridden.jpg");
    }

    [Fact]
    public void Transform_PostWithMultiplePreviewImages_SelectsLargestImage()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Multiple Preview Images",
                                Author = "testuser",
                                Preview = new RawRedditPreview
                                {
                                    Enabled = true,
                                    Images =
                                    [
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/small.jpg",
                                                Width = 400,
                                                Height = 300
                                            }
                                        },
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/large.jpg",
                                                Width = 1600,
                                                Height = 1200
                                            }
                                        },
                                        new RawRedditPreviewImage
                                        {
                                            Source = new RawRedditImageSource
                                            {
                                                Url = "https://preview.redd.it/medium.jpg",
                                                Width = 800,
                                                Height = 600
                                            }
                                        }
                                    ]
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://preview.redd.it/large.jpg");
    }

    [Fact]
    public void Transform_PostWithInvalidGalleryMetadata_SkipsInvalidImages()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Gallery with Invalid Metadata",
                                Author = "testuser",
                                IsGallery = true,
                                GalleryData = new RawRedditGalleryData
                                {
                                    Items =
                                    [
                                        new RawRedditGalleryItem { MediaId = "invalid" },
                                        new RawRedditGalleryItem { MediaId = "valid" }
                                    ]
                                },
                                MediaMetadata = new Dictionary<string, RawRedditMediaMetadata>
                                {
                                    ["invalid"] = new RawRedditMediaMetadata
                                    {
                                        Status = "invalid",
                                        Source = new RawRedditImageSource
                                        {
                                            Url = "https://i.redd.it/invalid.jpg",
                                            Width = 1000,
                                            Height = 800
                                        }
                                    },
                                    ["valid"] = new RawRedditMediaMetadata
                                    {
                                        Status = "valid",
                                        Source = new RawRedditImageSource
                                        {
                                            Url = "https://i.redd.it/valid.jpg",
                                            Width = 800,
                                            Height = 600
                                        }
                                    }
                                },
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            },
            new RawRedditListing { Data = new RawRedditListingData { Children = [] } }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Post.ImageUrl.ShouldBe("https://i.redd.it/valid.jpg");
    }

    [Fact]
    public void Transform_PostWithDefaultThumbnail_IgnoresThumbnail()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Default Thumbnail Post",
                                Author = "testuser",
                                Thumbnail = "default",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
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
    public void Transform_PostWithNsfwThumbnail_IgnoresThumbnail()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "NSFW Thumbnail Post",
                                Author = "testuser",
                                Thumbnail = "nsfw",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
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
    public void Transform_PostWithNonImageUrl_ReturnsNullImageUrl()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Non-Image URL Post",
                                Author = "testuser",
                                Url = "https://example.com/article.html",
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
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
    public void Transform_CommentsWithEmptyStringReplies_ReturnsEmptyList()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Test Post",
                                Author = "testuser",
                                CreatedUtc = DateTime.UtcNow
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
                                Body = "Comment with empty string replies",
                                Score = 10,
                                CreatedUtc = DateTime.UtcNow,
                                Replies = ""
                            }
                        }
                    ]
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
    public void Transform_CommentsWithNullListingData_ReturnsEmptyList()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Test Post",
                                Author = "testuser",
                                CreatedUtc = DateTime.UtcNow
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
                                Body = "Comment",
                                Score = 10,
                                CreatedUtc = DateTime.UtcNow,
                                Replies = new RawRedditListing { Data = null! }
                            }
                        }
                    ]
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
    public void Transform_CommentsWithNullChildren_ReturnsEmptyList()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Test Post",
                                Author = "testuser",
                                CreatedUtc = DateTime.UtcNow
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
                                Body = "Comment",
                                Score = 10,
                                CreatedUtc = DateTime.UtcNow,
                                Replies = new RawRedditListing
                                {
                                    Data = new RawRedditListingData { Children = null! }
                                }
                            }
                        }
                    ]
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
    public void Transform_CommentsWithNonT1Kind_SkipsComment()
    {
        // Arrange
        var redditPost = new RawRedditPost
        {
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
                                Title = "Test Post",
                                Author = "testuser",
                                CreatedUtc = DateTime.UtcNow
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
                            Kind = "more",
                            Data = new RawRedditCommentData
                            {
                                Id = "more123",
                                Author = "system",
                                CreatedUtc = DateTime.UtcNow
                            }
                        },
                        new RawRedditChild
                        {
                            Kind = "t1",
                            Data = new RawRedditCommentData
                            {
                                Id = "comment123",
                                Author = "commenter",
                                Body = "Valid comment",
                                Score = 10,
                                CreatedUtc = DateTime.UtcNow
                            }
                        }
                    ]
                }
            }
        };

        // Act
        var result = _transformer.Transform(redditPost);

        // Assert
        result.Comments.Count.ShouldBe(1);
        result.Comments[0].Id.ShouldBe("comment123");
    }
}
