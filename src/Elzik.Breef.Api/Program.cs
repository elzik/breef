using Elzik.Breef.Api.Auth;
using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Application;
using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure;
using Elzik.Breef.Infrastructure.AI;
using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Refit;
using Serilog;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        var configuration = builder.Configuration;
        configuration.AddEnvironmentVariables("breef_");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        builder.Host.UseSerilog();
        builder.Services.AddSerilog();

        Log.Information("Starting breef API v{Version}", GetProductVersion());

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

        builder.Services.Configure<BreefApiOptions>(configuration.GetSection("BreefApi"));
        builder.Services.AddAuth();

        builder.Services.Configure<WebPageDownLoaderOptions>(configuration.GetSection("WebPageDownLoader"));
        builder.Services.AddTransient<IWebPageDownloader, WebPageDownloader>();

        builder.Services.AddTransient<IContentExtractor, ContentExtractor>();

        builder.Services.Configure<AiServiceOptions>(configuration.GetSection("AiService"));
        builder.Services.Configure<AiContentSummariserOptions>(configuration.GetSection("AiContentSummariser"));


        builder.Services.AddAiContentSummariser();

        builder.Services.Configure<WallabagOptions>(configuration.GetSection("Wallabag"));
        AddWallabagBreefPublisher(builder.Services);

        builder.Services.AddTransient<IBreefGenerator, BreefGenerator>();

        var app = builder.Build();
        app.UseCors();
        app.UseAuth();

        app.AddBreefEndpoints();

        await app.RunAsync();
    }

    private static void AddWallabagBreefPublisher(IServiceCollection services)
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

    private static string GetProductVersion()
    {
        var assemblyAttributes = Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);

        if (assemblyAttributes.Length == 0)
        {
            throw new InvalidOperationException("No custom assembly attributes found; " +
                "unable to get informational version.");
        }

        return ((AssemblyInformationalVersionAttribute)assemblyAttributes[0]).InformationalVersion;
    }
}
