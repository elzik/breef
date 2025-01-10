using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Elzik.Breef.Api.Tests.Functional;

public class BreefTestsNative : BreefTestsBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public BreefTestsNative(WebApplicationFactory<Program> webAppFactory)
    {
        _webApplicationFactory = webAppFactory;

        _client = _webApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("BREEF-API-KEY", ApiKey);
    }

    private readonly HttpClient _client;

    public override HttpClient Client => _client;
}
