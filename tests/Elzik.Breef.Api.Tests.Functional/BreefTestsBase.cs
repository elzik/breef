using FluentAssertions;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace Elzik.Breef.Api.Tests.Functional
{
    public abstract class BreefTestsBase
    {
        public static string ApiKey = Guid.NewGuid().ToString();
        public string BaseUrl => $"http://localhost:{HostPort}";

        public virtual int HostPort { get; protected set; } = 8080;
        public abstract HttpClient Client { get; }
        protected virtual bool SkipTestsIf { get; }
        protected virtual string SkipTestsReason { get; } = "Test was skipped but no reason was given.";

        protected BreefTestsBase()
        {
            ApiKey = Guid.NewGuid().ToString();
            Environment.SetEnvironmentVariable("BREEF_API_KEY", ApiKey);
            Debug.WriteLine(this.GetType().Name);
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
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().Contain("http://example.com");
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
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().NotBeNullOrEmpty();
            responseString.Should().Contain("Unauthorised");
        }
    }
}