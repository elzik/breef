using Elzik.Breef.Domain;

namespace Elzik.Breef.Api.Presentation
{
    public static class PublishedBreefExtensions
    {
        public static PublishedBreefResponse ToPublishedBreefResponse(this PublishedBreef breef)
        {
            return new PublishedBreefResponse(breef.ResourceUrl, breef.PublishedUrl);
        }
    }
}
