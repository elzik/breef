using Elzik.Breef.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Elzik.Breef.Api.Tests.Functional;

public class ExceptionHandlingTests
{
    [Fact]
    public async Task Breefs_WithCallerFixableError_ReturnsExpectedProblemDetails()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/breefs");
        request.Headers.Add("BREEF-API-KEY", "test-key");
        request.Content = JsonContent.Create(new { url = "http://non-existent.elzik.co.uk" });

        // Act
        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var expected = new {
            title = "There was a problem with your request",
            status = 400,
            detail = "Failed to download content for URL: http://non-existent.elzik.co.uk"
        };
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("title").GetString().ShouldBe(expected.title);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(expected.status);
        json.RootElement.GetProperty("detail").GetString().ShouldBe(expected.detail);
        json.RootElement.TryGetProperty("traceId", out var traceIdProp).ShouldBeTrue();
        traceIdProp.GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Breefs_WithNonCallerFixableError_ReturnsGenericProblemDetails()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            ForceBreefGeneratorException(builder);
        });
        var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/breefs");
        request.Headers.Add("BREEF-API-KEY", "test-key");
        request.Content = JsonContent.Create(new { 
            url = "https://www.positive.news/society/swiping-less-living-more-how-to-take-control-of-our-digital-lives/" });

        // Act
        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var expected = new {
            title = "An error occurred while processing your request",
            status = 500,
            detail = "Contact your Breef administrator for a solution."
        };
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("title").GetString().ShouldBe(expected.title);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(expected.status);
        json.RootElement.GetProperty("detail").GetString().ShouldBe(expected.detail);
        json.RootElement.TryGetProperty("traceId", out var traceIdProp).ShouldBeTrue();
        traceIdProp.GetString().ShouldNotBeNullOrWhiteSpace();
    }

    private static void ForceBreefGeneratorException(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBreefGenerator));
            if (descriptor != null)
                services.Remove(descriptor);
            services.AddTransient<IBreefGenerator>(_ => throw new Exception("Forced test exception"));
        });
    }
}
