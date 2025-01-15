namespace Elzik.Breef.Domain
{
    public interface IContentExtractor
    {
        Task<string> Extract(string webPageUrl);
    }
}
