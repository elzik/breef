using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.Wallabag
{
    public class WallabagBreefPublisherTests
    {
        [Fact]
        public async Task Publish_WhenCalled_ShouldReturnPublishedBreef()
        {
            // Arrange
            var wallabagClient = Substitute.For<IWallabagClient>();
            var options = Options.Create(new WallabagOptions
            {
                BaseUrl = "https://test.com",
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                Username = "test-username",
                Password = "test-password"
            });
            var wallabagBreefPublisher = new WallabagBreefPublisher(wallabagClient, options);
            var breef = new Domain.Breef(
                "https://test.com", 
                "test-title", 
                "test-content", 
                "https://wallabag.elzik.co.uk/img/logo-wallabag.svg");
            var wallabagEntryCreateRequest = new WallabagEntryCreateRequest
            {
                Content = "test-content",
                Url = "https://test.com",
                Tags = "breef"
            };
            var wallabagEntryID = 123;
            var wallabagEntry = new WallabagEntry
            {
                IsArchived = 0,
                IsStarred = 0,
                UserName = "test-username",
                UserEmail = "test-email@test.com",
                UserId = 1,
                Tags = [],
                IsPublic = false,
                Id = wallabagEntryID,
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
                        Href = $"/api/entries/{wallabagEntryID}"
                    }
                },
                Headers = []
            };
            wallabagClient.PostEntryAsync(Arg.Is<WallabagEntryCreateRequest>(r =>
                r.Content == "test-content" &&
                r.Url == "https://test.com" &&
                r.Tags == "breef")).Returns(wallabagEntry);

            // Act
            var result = await wallabagBreefPublisher.PublishAsync(breef);

            // Assert
            result.PublishedUrl.ShouldBe($"{options.Value.BaseUrl}/view/{wallabagEntry.Id}");
            result.ResourceUrl.ShouldBe($"{options.Value.BaseUrl}{wallabagEntry.Links.Self.Href}");
        }
    }
}
