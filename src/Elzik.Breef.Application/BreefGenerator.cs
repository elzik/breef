using System.Diagnostics;

namespace Elzik.Breef.Application
{
    public class BreefGenerator : IBreefGenerator
    {
        public async Task<Domain.PublishedBreef> GenerateBreefAsync(string url)
        {
            var breef = new Domain.PublishedBreef(url);

            Debug.WriteLine(DateTime.Now.TimeOfDay.TotalNanoseconds + ": " + url);

            return breef;
        }
    }
}
