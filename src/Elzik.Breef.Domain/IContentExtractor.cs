namespace Elzik.Breef.Domain
{
    public interface IContentExtractor
    {
        Task<Extract> ExtractAsync(string webPageUrl);
    }
}
