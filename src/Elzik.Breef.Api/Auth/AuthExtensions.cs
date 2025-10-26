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

        var authBuilder = services.AddAuthorizationBuilder();
        authBuilder.AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser());

        services.Configure<AuthorizationOptions>(options =>
        {
            options.FallbackPolicy = options.GetPolicy("RequireAuthenticated");
        });
    }

    public static void UseAuth(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
