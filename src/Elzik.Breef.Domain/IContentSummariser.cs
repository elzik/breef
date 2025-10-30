namespace Elzik.Breef.Domain
{
    public interface IContentSummariser
    {
        Task<string> SummariseAsync(string content, string instructions);
    }
}
