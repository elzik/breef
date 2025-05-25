namespace Elzik.Breef.Domain
{
    public interface IHttpDownloader
    {
        Task<string> DownloadAsync(string url);
    }
}