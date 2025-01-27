using Refit;

namespace Elzik.Breef.Infrastructure.Wallabag;

public interface IWallabagClient
{
    [Post("/oauth/v2/token")]
    Task<TokenResponse> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] TokenRequest tokenRequest);

    [Post("/api/entries")]
    [Headers("Authorization: Bearer")]
    Task<WallabagEntry> PostEntryAsync([Body] WallabagEntryCreateRequest entry);
}
