using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Elzik.Breef.Api.Tests.Functional;

public class HealthTestsNative
{
    private readonly ITestOutputHelper _output;

    public HealthTestsNative(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Health_Called_ReturnsOK(string environmentName)
    {
        // Arrange
        _output.WriteLine($"Testing health endpoint in {environmentName} environment");

        using WebApplicationFactory<Program> factory = environmentName == "Development"
            ? new DevelopmentWebApplicationFactory()
            : new ProductionWebApplicationFactory();

        using var client = factory.CreateClient();
        const string baseUrl = "http://localhost";

        // Act
        var response = await client.GetAsync($"{baseUrl}/health");

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
