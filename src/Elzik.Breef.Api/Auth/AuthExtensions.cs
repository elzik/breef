using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authorization;

namespace Elzik.Breef.Api.Auth;

public static class AuthExtensions
{
    public static void AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeader<BreefApiKeyProvider>(options =>
            {
                options.KeyName = "BREEF-API-KEY";
                options.Realm = "BreefAPI";
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    public static void UseAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
