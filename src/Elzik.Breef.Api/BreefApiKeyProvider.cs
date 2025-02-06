using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Options;

namespace Elzik.Breef.Api;

public class BreefApiKeyProvider(IOptions<BreefApiOptions> breefOptions) : IApiKeyProvider
{
    private readonly BreefApiOptions _breefOptions = breefOptions.Value
            ?? throw new ArgumentNullException(nameof(breefOptions));

    public Task<IApiKey?> ProvideAsync(string key)
    {
        if (key == _breefOptions.ApiKey)
        {
            return Task.FromResult<IApiKey?>(new ApiKey(key));
        }

        return Task.FromResult<IApiKey?>(null);
    }
}
