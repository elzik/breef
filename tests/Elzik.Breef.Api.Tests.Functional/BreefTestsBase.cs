using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Domain;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Elzik.Breef.Api.Tests.Functional
{
    public abstract class BreefTestsBase
    {
        protected static string? ApiKey { get; private set; }
        protected string BaseUrl => $"http://localhost:{HostPort}";

        protected virtual int HostPort { get; set; } = 8080;
        protected abstract HttpClient Client { get; }
        protected virtual bool SkipTestsIf { get; }
        protected virtual string SkipTestsReason { get; } = "Test was skipped but no reason was given.";

        protected BreefTestsBase()
        {
            var apiKeyEnvironmentVariableName = "breef_BreefApi__ApiKey";
            ApiKey = Environment.GetEnvironmentVariable(apiKeyEnvironmentVariableName)
                ?? throw new InvalidOperationException($"{apiKeyEnvironmentVariableName} environment variable must contain an API key.");
        }


        [SkippableFact]
        public async Task EndToEndHappyPath()
        {
            Skip.If(SkipTestsIf, SkipTestsReason);

            // Arrange
            var breef = new { Url = $"https://example.com" };

            // Act
            var response = await Client.PostAsJsonAsync($"{BaseUrl}/breefs", breef);

            // Assert
            var wallabagBaseUrl = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_URL");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNullOrEmpty();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var publishedBreef = JsonSerializer.Deserialize<PublishedBreefResponse>(responseString, options); 
            publishedBreef.ShouldNotBeNull();
            publishedBreef.Url.ShouldStartWith($"{wallabagBaseUrl}/api/entries/");
        }

        [SkippableFact]
        public async Task Unauthorised()
        {
            Skip.If(SkipTestsIf, SkipTestsReason);

            // Arrange
            Client.DefaultRequestHeaders.Clear();
            var breef = new { Url = "http://example.com" };

            // Act
            var response = await Client.PostAsJsonAsync($"{BaseUrl}/breefs", breef);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNullOrEmpty();
            responseString.ShouldContain("Unauthorised");
        }
    }
}