using AspNetCore.Authentication.ApiKey;

namespace Elzik.Breef.Api
{
    public static class AuthExtensions
    {
        public static void AddAuth(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
                .AddApiKeyInHeader<EnvironmentApiKeyProvider>(options =>
                {
                    options.KeyName = "BREEF-API-KEY";
                    options.Realm = "BreefAPI";
                });
            builder.Services.AddAuthorization();
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
