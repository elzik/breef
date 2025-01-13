using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure
{
    public sealed class HttpClientWrapper : IHttpClient, IDisposable
    {
        private readonly HttpClient _httpClient = new();

        public async Task<string> GetStringAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
