using Elzik.Breef.Application;

namespace Elzik.Breef.Api.Presentation
{
    public static class BreefEndpointsExtensions
    {
        public static void AddBreefEndpoints(this WebApplication app)
        {
            var breefs = app.MapGroup("/breefs");

            breefs.MapPost("/", async (IBreefGenerator breefGenerator, SourcePageRequest sourcePage) =>
            {
                var breef  = await breefGenerator.GenerateBreefAsync(sourcePage.Url);

                return breef.ToBreefResonse();
            });
        }
    }
}
