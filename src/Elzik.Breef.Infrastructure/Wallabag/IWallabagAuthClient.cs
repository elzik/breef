using Refit;

namespace Elzik.Breef.Infrastructure.Wallabag;

public interface IWallabagAuthClient
{
    [Post("/oauth/v2/token")]
    Task<TokenResponse> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] TokenRequest tokenRequest);
}
