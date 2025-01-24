namespace Elzik.Breef.Domain
{
    public interface IContentSummariser
    {
        Task<string> Summarise(string content);
    }
}
