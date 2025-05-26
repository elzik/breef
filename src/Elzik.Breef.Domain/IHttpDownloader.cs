namespace Elzik.Breef.Domain
{
    public interface IHttpDownloader
    {
        Task<bool> TryGet(string url);
        Task<string> DownloadAsync(string url);
    }
}