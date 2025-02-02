using Refit;

namespace Elzik.Breef.Infrastructure.Wallabag;

public interface IWallabagClient
{
    [Post("/api/entries")]
    [Headers("Authorization: Bearer")]
    Task<WallabagEntry> PostEntryAsync([Body] WallabagEntryCreateRequest entry);
}
