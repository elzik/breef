using Elzik.Breef.Infrastructure.Wallabag;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Elzik.Breef.Infrastructure.Tests.Unit.Wallabag
{
    public class WallabagBreefPublisherTests
    {
        [Fact]
        public async Task Publish_WhenCalled_ShouldReturnPublishedBreef()
        {
            // Arrange
            var wallabagClient = Substitute.For<IWallabagClient>();
            var options = Options.Create(new WallabagOptions(
                "https://test.com",
                "test-client-id",
                "test-client-secret",
                "test-username",
                "test-password"));
            var wallabagBreefPublisher = new WallabagBreefPublisher(wallabagClient, options);
            var breef = new Domain.Breef("https://test.com", "test-title", "test-content");
            var wallabagEntryCreateRequest = new WallabagEntryCreateRequest
            {
                Content = "test-content",
                Url = "https://test.com",
                Tags = "breef"
            };
            var wallabagEntry = new WallabagEntry
            {
                IsArchived = 0,
                IsStarred = 0,
                UserName = "test-username",
                UserEmail = "test-email@test.com",
                UserId = 1,
                Tags = [],
                IsPublic = false,
                Id = 1,
                Title = "test-title",
                Url = "https://test.com",
                HashedUrl = "hashed-url",
                GivenUrl = "https://test.com",
                HashedGivenUrl = "hashed-given-url",
                Content = "test-content",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedBy = [],
                Annotations = [],
                ReadingTime = 5,
                DomainName = "test.com",
                Links = new Links
                {
                    Self = new Self
                    {
                        Href = "/entry/1"
                    }
                }
            };
            wallabagClient.PostEntryAsync(Arg.Is<WallabagEntryCreateRequest>(r => 
                r.Content == "test-content" &&
                r.Url == "https://test.com" &&
                r.Tags == "breef")).Returns(wallabagEntry);

            // Act
            var result = await wallabagBreefPublisher.PublishAsync(breef);

            // Assert
            result.PublishedUrl.Should().Be($"{options.Value.BaseUrl}{wallabagEntry.Links.Self.Href}");
        }
    }
}
