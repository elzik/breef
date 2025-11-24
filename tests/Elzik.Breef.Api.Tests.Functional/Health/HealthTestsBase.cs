using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Elzik.Breef.Api.Tests.Functional.Health
{
    public abstract class HealthTestsBase
    {
        protected string BaseUrl => $"http://localhost:{HostPort}";
        protected virtual int HostPort { get; set; } = 8080;
        protected abstract HttpClient Client { get; }
        protected virtual bool SkipTestsIf { get; }
        protected virtual string SkipTestsReason { get; } = "Test was skipped but no reason was given.";

        [SkippableFact]
        public async Task Health_Called_ReturnsOK()
        {
            Skip.If(SkipTestsIf, SkipTestsReason);

            // Act
            var response = await Client.GetAsync($"{BaseUrl}/health");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
            body.ShouldNotBeNull();
            body!.Status.ShouldBe("Healthy");
        }

        private class HealthResponse
        {
            public string Status { get; set; } = string.Empty;
        }
    }
}
