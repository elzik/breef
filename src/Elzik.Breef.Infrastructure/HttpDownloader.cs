using Elzik.Breef.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elzik.Breef.Infrastructure
{
    public sealed class HttpDownloader : IHttpDownloader, IDisposable
    {
        private readonly HttpClient _httpClient;

        public HttpDownloader(ILogger<HttpDownloader> logger,  
            IOptions<HttpDownloaderOptions> HttpDownloaderOptions)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", HttpDownloaderOptions.Value.UserAgent);

            logger.LogInformation("Downloads will be made using the User-Agent: {UserAgent}", 
                _httpClient.DefaultRequestHeaders.UserAgent);
        }

        public async Task<string> DownloadAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public async Task<bool> TryGet(string url)
        {
            if(string.IsNullOrWhiteSpace(url)) return false;

            var response = await _httpClient.GetAsync(url);

            return response.IsSuccessStatusCode;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
