using Refit;

namespace Elzik.Breef.Infrastructure.Wallabag;

public interface IWallabagClient
{
    [Post("/oauth/v2/token")]
    Task<TokenResponse> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] TokenRequest tokenRequest);
}
