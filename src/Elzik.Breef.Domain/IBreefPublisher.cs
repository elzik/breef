namespace Elzik.Breef.Domain
{
    public interface IBreefPublisher
    {
        Task<PublishedBreef> PublishAsync(Breef breef);
    }
}
