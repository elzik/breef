using Elzik.Breef.Domain;
using Microsoft.Extensions.Options;

namespace Elzik.Breef.Infrastructure.Wallabag
{
    public class WallabagBreefPublisher(IWallabagClient WallabagClient, IOptions<WallabagOptions> options) 
        : IBreefPublisher
    {
        public async Task<PublishedBreef> PublishAsync(Domain.Breef breef)
        {
            WallabagEntryCreateRequest wallabagEntryCreateRequest = new()
            {
                Content = breef.Content,
                Url = breef.OrigUrl,
                Tags = "breef"
            };

            var wallabagEntry = await WallabagClient.PostEntryAsync(wallabagEntryCreateRequest);

            var publishedUriBuilder = new UriBuilder(options.Value.BaseUrl)
            {
                Path = wallabagEntry.Links.Self.Href,
                Port = -1
            };

            var publishedBreef = new PublishedBreef(publishedUriBuilder.ToString());

            return publishedBreef;
        }
    }
}
