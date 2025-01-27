namespace Elzik.Breef.Api.Presentation
{
    public static class BreefExtensions
    {
        public static BreefResponse ToBreefResonse(this Domain.PublishedBreef breef)
        {
            return new BreefResponse(breef.PublishedUrl);
        }
    }
}
