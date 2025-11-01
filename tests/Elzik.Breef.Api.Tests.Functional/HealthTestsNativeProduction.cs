using Microsoft.AspNetCore.Mvc.Testing;
using System;

namespace Elzik.Breef.Api.Tests.Functional;

public class HealthTestsNativeProduction : HealthTestsBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly HttpClient _client;

    protected override HttpClient Client => _client;

    public HealthTestsNativeProduction(WebApplicationFactory<Program> webAppFactory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        _webApplicationFactory = webAppFactory;
        _client = _webApplicationFactory.CreateClient();
    }
}
