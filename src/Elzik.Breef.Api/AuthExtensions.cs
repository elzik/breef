using AspNetCore.Authentication.ApiKey;
using Elzik.Breef.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Elzik.Breef.Api
{
    public static class AuthExtensions
    {
        public static void AddAuth(this IServiceCollection services, IConfigurationManager configurationManager)
        {
            services.Configure<BreefApiOptions>(configurationManager.GetSection("BreefApi"));

            services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
                .AddApiKeyInHeader<BreefApiKeyProvider>(options =>
                {
                    options.KeyName = "BREEF-API-KEY";
                    options.Realm = "BreefAPI";
                });
            services.AddAuthorization();
        }

        public static void UseAuth(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.Use(async (context, next) =>
            {
                if (context.User.Identity != null && !context.User.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorised");
                    return;
                }
                await next();
            });
        }
    }
}
