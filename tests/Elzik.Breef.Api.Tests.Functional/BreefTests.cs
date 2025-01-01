using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Elzik.Breef.Api.Tests.Functional;

public class BreefTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BreefTests(WebApplicationFactory<Program> webAppFactory)
    {
        _client = webAppFactory.CreateClient();
        var apiKey = Guid.NewGuid().ToString();
        Environment.SetEnvironmentVariable("BREEF_API_KEY", apiKey);
        _client.DefaultRequestHeaders.Add("BREEF-API-KEY", apiKey);
    }

    [Fact]
    public async Task EndToEndHappyPath()
    {
        // Arrange
        var breef = new { Url = "http://example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/breefs", breef);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        responseString.Should().Contain("http://example.com");
    }

    [Fact]
    public async Task Unauthorised()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        var breef = new { Url = "http://example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/breefs", breef);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        responseString.Should().Contain("Unauthorised");
    }
}
