using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Elzik.Breef.Api.Tests.Functional
{
 public class HealthEndpointTests : IClassFixture<Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Elzik.Breef.Api.Program>>
 {
 private readonly HttpClient _client;

 public HealthEndpointTests(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Elzik.Breef.Api.Program> factory)
 {
 _client = factory.CreateClient();
 }

 [Fact]
 public async Task Health_Called_ReturnsOK()
 {
 // Act
 var response = await _client.GetAsync("/health");

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
