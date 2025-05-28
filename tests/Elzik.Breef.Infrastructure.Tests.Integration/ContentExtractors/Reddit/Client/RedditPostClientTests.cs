using System.Threading.Tasks;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Refit;
using Shouldly;
using Xunit;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors.Reddit.Client
{
    public class RedditPostClientTests
    {
        [Fact]
        public async Task GetPost_ValidPostId_ReturnsRedditPost()
        {
            // Arrange
            var client = RestService.For<IRedditPostClient>("https://www.reddit.com/");
            var postId = "1dtr46l";

            // Act
            var redditPost = await client.GetPost(postId);

            // Assert
            redditPost.ShouldNotBeNull();
        }
    }
}