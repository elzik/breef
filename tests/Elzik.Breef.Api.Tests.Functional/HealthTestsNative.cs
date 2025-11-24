using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Elzik.Breef.Api.Tests.Functional;

public class HealthTestsNative : HealthTestsBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly HttpClient _client;

    protected override HttpClient Client => _client;

    public HealthTestsNative(WebApplicationFactory<Program> webAppFactory)
    {
        _webApplicationFactory = webAppFactory;
        _client = _webApplicationFactory.CreateClient();
    }
}
