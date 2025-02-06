using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration
{
    public class HttpMessageXunitLoggingHandler(ITestOutputHelper output) : DelegatingHandler(new HttpClientHandler())
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            output.WriteLine($"Request: {request}");

            if (request.Content != null)
            {
                output.WriteLine($"Request Content: {await request.Content.ReadAsStringAsync(cancellationToken)}");
            }

            var response = await base.SendAsync(request, cancellationToken);

            output.WriteLine($"Response: {response}");
            if (response.Content != null)
            {
                output.WriteLine($"Response Content: {await response.Content.ReadAsStringAsync(cancellationToken)}");
            }

            return response;
        }
    }
}
