using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure
{
    public class CallerFixableHttpRequestException(string message, Exception? innerException = null) 
        : HttpRequestException(message, innerException), ICallerFixableException
    {
    }
}
