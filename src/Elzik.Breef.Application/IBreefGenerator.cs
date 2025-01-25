namespace Elzik.Breef.Application
{
    public interface IBreefGenerator
    {
        Task<Domain.PublishedBreef> GenerateBreefAsync(string url);
    }
}
