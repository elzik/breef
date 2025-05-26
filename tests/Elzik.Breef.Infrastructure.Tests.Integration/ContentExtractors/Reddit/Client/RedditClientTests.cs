using System.Threading.Tasks;
using Refit;
using Shouldly;
using Xunit;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors.Reddit.Client
{
    public class RedditClientTests
    {
        [Fact]
        public async Task GetNewInSubReddit_ValidSUbReddit_ReturnsNewInSubreddit()
        {
            // Arrange
            var client = RestService.For<ISubredditClient>("https://www.reddit.com/");

            // Act
            var newInSubreddit = await client.GetNewInSubreddit("reddit");

            // Assert
            newInSubreddit.ShouldNotBeNull();
            newInSubreddit.Data.ShouldNotBeNull();
            newInSubreddit.Data.Children.ShouldNotBeNull();
            newInSubreddit.Data.Children.Count.ShouldBe(25);
        }
    }
}
