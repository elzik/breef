using System.Diagnostics;

namespace Elzik.Breef.Api
{
    public class HttpMessageDebugLoggingHandler() : DelegatingHandler(new HttpClientHandler())
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine($"Request: {request}");

            if (request.Content != null)
            {
                Debug.WriteLine($"Request Content: {await request.Content.ReadAsStringAsync(cancellationToken)}");
            }

            var response = await base.SendAsync(request, cancellationToken);

            Debug.WriteLine($"Response: {response}");
            if (response.Content != null)
            {
                Debug.WriteLine($"Response Content: {await response.Content.ReadAsStringAsync(cancellationToken)}");
            }

            return response;
        }
    }
}
