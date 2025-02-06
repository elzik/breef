using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Configuration;
using Refit;
using Shouldly;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration.Wallabag
{
    public class WallabagAuthClientTests
    {
        private readonly IWallabagAuthClient? _wallabagAuthClient;
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly WallabagOptions _wallabagOptions;

        public WallabagAuthClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper
                ?? throw new ArgumentNullException(nameof(testOutputHelper));

            
            var refitSettings = new RefitSettings();
            if (Environment.GetEnvironmentVariable("BREEF_TESTS_ENABLE_HTTP_MESSAGE_LOGGING") == "true")
            {
                refitSettings.HttpMessageHandlerFactory = () => new HttpMessageXunitLoggingHandler(_testOutputHelper);
            }

            var configuration = new ConfigurationBuilder()
               .AddEnvironmentVariables("breef_")
               .Build();
            _wallabagOptions = configuration.GetRequiredSection("Wallabag").Get<WallabagOptions>()
                ?? throw new InvalidOperationException("Wallabag options not found in Wallabag configuration section.");

            _wallabagAuthClient = RestService.For<IWallabagAuthClient>(_wallabagOptions.BaseUrl, refitSettings);
            if (_wallabagAuthClient == null)
            {
                throw new InvalidOperationException("Failed to create Wallabag client.");
            }
        }

        [Fact]
        public async Task GetTokenAsync_FromTestWallabagAccount_ReturnsToken()
        {
            // Arrange
            var testWallabagTokenRequest = new TokenRequest
            {
                ClientId = _wallabagOptions.ClientId,
                ClientSecret = _wallabagOptions.ClientSecret,
                Username = _wallabagOptions.Username,
                Password = _wallabagOptions.Password
            };

            // Act
            var tokenResponse = await _wallabagAuthClient!.GetTokenAsync(testWallabagTokenRequest!); // ctor will fail if _wallabagTokenRequest is null

            // Assert
            tokenResponse.ShouldNotBeNull();
            tokenResponse.AccessToken.ShouldNotBeNullOrEmpty();
            tokenResponse.RefreshToken.ShouldNotBeNullOrEmpty();
            tokenResponse.TokenType.ShouldBe("bearer");
        }
    }
}
