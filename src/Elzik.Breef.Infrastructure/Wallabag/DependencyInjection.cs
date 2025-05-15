using Elzik.Breef.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Elzik.Breef.Infrastructure.Wallabag;

public static class DependencyInjection
{
    public static void AddWallabagBreefPublisher(this IServiceCollection services)
    {
        services.AddRefitClient<IWallabagAuthClient>();

        var wallabagOptions = services.BuildServiceProvider()
            .GetRequiredService<IOptions<WallabagOptions>>().Value;

        var refitSettings = new RefitSettings
        {
            AuthorizationHeaderValueGetter = async (request, cancellationToken) =>
            {
                var wallabagClient = RestService.For<IWallabagAuthClient>(wallabagOptions.BaseUrl);

                var tokenRequest = new TokenRequest
                {
                    ClientId = wallabagOptions.ClientId,
                    ClientSecret = wallabagOptions.ClientSecret,
                    Username = wallabagOptions.Username,
                    Password = wallabagOptions.Password
                };

                var tokenResponse = await wallabagClient.GetTokenAsync(tokenRequest);
                return tokenResponse.AccessToken;
            },
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            })
        };

#if DEBUG
        refitSettings.HttpMessageHandlerFactory = () => new HttpMessageDebugLoggingHandler();
#endif

        services.AddRefitClient<IWallabagClient>(refitSettings)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(wallabagOptions.BaseUrl);
            });

        services.AddTransient<IBreefPublisher, WallabagBreefPublisher>();
    }
}
