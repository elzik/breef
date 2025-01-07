using AspNetCore.Authentication.ApiKey;
using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Application;
using System.Diagnostics;

namespace Elzik.Breef.Api;

public class Program
{
    protected Program()
    {
        // This satisfies Sonar's S1118: Utility classes should not have public constructors
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
        builder.AddAuth();

        builder.Services.AddTransient<IBreefGenerator, BreefGenerator>();


        var app = builder.Build();
        app.UseCors();
        app.UseAuth();

        app.AddBreefEndpoints();

        await app.RunAsync();
    }
}
