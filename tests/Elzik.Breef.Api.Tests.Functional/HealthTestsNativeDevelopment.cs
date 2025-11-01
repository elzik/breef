using Microsoft.AspNetCore.Mvc.Testing;

namespace Elzik.Breef.Api.Tests.Functional;

public class HealthTestsNativeDevelopment : HealthTestsBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly HttpClient _client;

    protected override HttpClient Client => _client;

    public HealthTestsNativeDevelopment(WebApplicationFactory<Program> webAppFactory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        _webApplicationFactory = webAppFactory;
        _client = _webApplicationFactory.CreateClient();
    }
}
