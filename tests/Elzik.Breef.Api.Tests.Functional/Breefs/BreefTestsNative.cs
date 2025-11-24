using Microsoft.AspNetCore.Mvc.Testing;

namespace Elzik.Breef.Api.Tests.Functional.Breefs;

public class BreefTestsNative : BreefTestsBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly HttpClient _client;

    protected override HttpClient Client => _client;

    public BreefTestsNative(WebApplicationFactory<Program> webAppFactory)
    {
        _webApplicationFactory = webAppFactory;

        _client = _webApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("BREEF-API-KEY", ApiKey);
    }
}
