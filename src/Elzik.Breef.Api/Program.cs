using AspNetCore.Authentication.ApiKey;
using System.Diagnostics;

namespace Elzik.Breef.Api;

public class Program
{
    protected Program()
    {
        // This satisfies SonarQube's S1118: Utility classes should not have public constructors
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });
        builder.Services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeader<EnvironmentApiKeyProvider>(options =>
            {
                options.KeyName = "BREEF-API-KEY";
                options.Realm = "BreefAPI";
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.UseCors();
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

        var breefs = app.MapGroup("/breefs");
        breefs.MapPost("/", async (Breef breef) =>
        {
            Debug.WriteLine(DateTime.Now.TimeOfDay.TotalNanoseconds + ": " + breef.Url);

            return Results.Created(breef.Url, breef);
        });

        await app.RunAsync();
    }
}
