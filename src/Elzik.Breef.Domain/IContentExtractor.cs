namespace Elzik.Breef.Domain
{
    public interface IContentExtractor
    {
        Task<string> ExtractAsync(string webPageUrl);
    }
}
