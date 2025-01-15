namespace Elzik.Breef.Domain
{
    public interface IWebPageDownloader
    {
        Task<string> DownloadAsync(string url);
    }
}