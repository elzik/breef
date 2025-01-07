using System.Diagnostics;

namespace Elzik.Breef.Application
{
    public class BreefGenerator : IBreefGenerator
    {
        public async Task<Domain.Breef> GenerateBreefAsync(string url)
        {
            var breef = new Domain.Breef(url);

            Debug.WriteLine(DateTime.Now.TimeOfDay.TotalNanoseconds + ": " + url);

            return breef;
        }
    }
}
