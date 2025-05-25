namespace Elzik.Breef.Domain
{
    public interface IContentExtractor
    {
        bool CanHandle(string webPageUrl);

        Task<Extract> ExtractAsync(string webPageUrl);
    }
}
