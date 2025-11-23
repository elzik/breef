using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure
{
    public class CallerFixableHttpRequestException : HttpRequestException, ICallerFixableException
    {
        public CallerFixableHttpRequestException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
