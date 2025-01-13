namespace Elzik.Breef.Domain
{
    public interface IHttpClient
    {
        Task<string> GetStringAsync(string url);
    }
}