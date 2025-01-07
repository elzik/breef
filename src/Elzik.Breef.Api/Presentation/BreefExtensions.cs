namespace Elzik.Breef.Api.Presentation
{
    public static class BreefExtensions
    {
        public static BreefResponse ToBreefResonse(this Domain.Breef breef)
        {
            return new BreefResponse(breef.Url);
        }
    }
}
