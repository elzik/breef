namespace Elzik.Breef.Application
{
    public interface IBreefGenerator
    {
        Task<Domain.Breef> GenerateBreefAsync(string url);
    }
}
