using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure
{
    public sealed class WebPageDownloader : IWebPageDownloader, IDisposable
    {
        private readonly HttpClient _httpClient = new();

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
