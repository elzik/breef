using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Elzik.Breef.Api.Tests.Functional
{
    public abstract class BreefTestsBase
    {
        protected static string ApiKey { get; private set; } = ApiKey = Guid.NewGuid().ToString();
        protected string BaseUrl => $"http://localhost:{HostPort}";

        protected virtual int HostPort { get; set; } = 8080;
        protected abstract HttpClient Client { get; }
        protected virtual bool SkipTestsIf { get; }
        protected virtual string SkipTestsReason { get; } = "Test was skipped but no reason was given.";

        protected BreefTestsBase()
        {
            Environment.SetEnvironmentVariable("BREEF_API_KEY", ApiKey);
        }


        [SkippableFact]
        public async Task EndToEndHappyPath()
        {
            Skip.If(SkipTestsIf, SkipTestsReason);

            // Arrange
            var breef = new { Url = "http://example.com" };

            // Act
            var response = await Client.PostAsJsonAsync($"{BaseUrl}/breefs", breef);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNullOrEmpty();
            responseString.ShouldContain("http://example.com");
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