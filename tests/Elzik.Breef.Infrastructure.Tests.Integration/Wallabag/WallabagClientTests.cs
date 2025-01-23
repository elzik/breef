using Refit;
using Elzik.Breef.Infrastructure.Wallabag;
using FluentAssertions;

namespace Elzik.Breef.Infrastructure.Tests.Integration.Wallabag
{
    public class WallabagClientTests
    {
        private readonly IWallabagClient _tokenClient;

        private readonly string _wallabagUrl;
        private readonly string _wallaClientId;
        private readonly string _wallabagClientSecret;
        private readonly string _wallabagUsername;
        private readonly string _wallabagPassword;

        public WallabagClientTests()
        {
            _wallabagUrl = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_URL")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_URL must contain a Wallabag URL");

            _wallaClientId = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_CLIENT_ID")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_CLIENT_ID must contain a Wallabag client ID");

            _wallabagClientSecret = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_CLIENT_SECRET")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_CLIENT_SECRET must contain a Wallabag client secret");

            _wallabagUsername = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_USERNAME")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_USERNAME must contain a Wallabag username");
            
            _wallabagPassword = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_PASSWORD")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_PASSWORD must contain a Wallabag password");

            _tokenClient = RestService.For<IWallabagClient>(_wallabagUrl);
        }

        [Fact]
        public async Task GetTokenAsync_FromTestWallabagAccount_ReturnsToken()
        {
            // Arrange
            var tokenRequest = new TokenRequest
            {
                ClientId = _wallaClientId,
                ClientSecret = _wallabagClientSecret,
                Username = _wallabagUsername,
                Password = _wallabagPassword
            };

            // Act
            var tokenResponse = await _tokenClient.GetTokenAsync(tokenRequest);

            // Assert
            tokenResponse.Should().NotBeNull();
            tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
            tokenResponse.RefreshToken.Should().NotBeNullOrEmpty();
            tokenResponse.TokenType.Should().Be("bearer");
        }
    }
}
