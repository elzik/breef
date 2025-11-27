using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Elzik.Breef.Api.Tests.Functional.Breefs
{
    public abstract class BreefTestsBase
    {
        protected static string? ApiKey { get; private set; }
        protected string BaseUrl => $"http://localhost:{HostPort}";

        protected virtual int HostPort { get; set; } = 8080;
        protected abstract HttpClient Client { get; }
        protected virtual bool SkipTestsIf { get; }
        protected virtual string SkipTestsReason { get; } = "Test was skipped but no reason was given.";

        private readonly WallabagOptions _wallabagOptions;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected BreefTestsBase()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("breef_")
                .Build();
            _wallabagOptions = configuration.GetRequiredSection("Wallabag").Get<WallabagOptions>() 
                ?? throw new InvalidOperationException("Wallabag options not found in Wallabag configuration section.");
            var breefApiOptions = configuration.GetRequiredSection("BreefApi").Get<BreefApiOptions>()
                ?? throw new InvalidOperationException("Breef API options not found in BreefApi configuration section.");
            ApiKey = breefApiOptions.ApiKey;
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
            var publishedBreef = JsonSerializer
                .Deserialize<PublishedBreefResponse>(responseString, JsonSerializerOptions);
            publishedBreef.ShouldNotBeNull();
            publishedBreef.ResourceUrl.ShouldStartWith($"{_wallabagOptions.BaseUrl}/api/entries/");
            publishedBreef.PublishedUrl.ShouldStartWith($"{_wallabagOptions.BaseUrl}/view/");
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
            response.Headers.WwwAuthenticate.ShouldNotBeEmpty();
            var challenge = response.Headers.WwwAuthenticate.First();
            challenge.Scheme.ShouldBe("ApiKey");
            challenge.Parameter.ShouldNotBeNullOrEmpty();
            challenge.Parameter.ShouldContain("BREEF-API-KEY");
            
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldNotBeNullOrEmpty();
            var problemDetails = JsonSerializer
                .Deserialize<ProblemDetails>(responseString, JsonSerializerOptions);
            problemDetails.ShouldNotBeNull();
            problemDetails.Status.ShouldBe(401);
            problemDetails.Title.ShouldBe("Unauthorized");
            problemDetails.Type.ShouldNotBeNullOrWhiteSpace();
        }
    }
}