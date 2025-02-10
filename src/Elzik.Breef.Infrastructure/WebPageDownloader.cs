using Elzik.Breef.Domain;
using Microsoft.Extensions.Options;

namespace Elzik.Breef.Infrastructure
{
    public sealed class WebPageDownloader : IWebPageDownloader, IDisposable
    {
        private readonly HttpClient _httpClient;

        public WebPageDownloader(IOptions<WebPageDownLoaderOptions> WebPageDownLoaderOptions)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", WebPageDownLoaderOptions.Value.UserAgent);
        }

        public async Task<string> DownloadAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
