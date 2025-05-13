using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Application;
using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure;
using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Refit;
using Serilog;
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
        builder.Services.AddAuth(configuration);

        builder.Services.Configure<WebPageDownLoaderOptions>(configuration.GetSection("WebPageDownLoader"));
        builder.Services.AddTransient<IWebPageDownloader, WebPageDownloader>();

        builder.Services.AddTransient<IContentExtractor, ContentExtractor>();

        builder.Services.Configure<AiContentSummariserOptions>(configuration.GetSection("AiContentSummariser"));
        builder.Services.AddTransient<IContentSummariser, AiContentSummariser>();

        AddAiService(builder.Services, configuration);
        AddWallabagBreefPublisher(builder.Services, configuration);
        builder.Services.AddTransient<IBreefGenerator, BreefGenerator>();

        var app = builder.Build();
        app.UseCors();
        app.UseAuth();

        app.AddBreefEndpoints();

        await app.RunAsync();
    }

    private static void AddAiService(IServiceCollection services, IConfigurationManager configuration)
    {
        services.Configure<AiServiceOptions>(configuration.GetSection("AiService"));

        var aiServiceOptions = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AiServiceOptions>>().Value;

        var kernelBuilder = aiServiceOptions.Provider switch
        {
            AiServiceProviders.OpenAI => 
                Kernel.CreateBuilder().AddOpenAIChatCompletion(aiServiceOptions.ModelId, new Uri(aiServiceOptions.EndpointUrl), aiServiceOptions.ApiKey),
            AiServiceProviders.AzureOpenAI => 
                Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(aiServiceOptions.ModelId, aiServiceOptions.EndpointUrl, aiServiceOptions.ApiKey), 
            AiServiceProviders.NotSet => 
                throw new InvalidOperationException("AiService provider is not set."),
            _ => 
                throw new InvalidOperationException($"Unsupported AiService provider: {aiServiceOptions.Provider}"),
        };

        kernelBuilder.Services.AddSerilog();
        var kernel = kernelBuilder.Build();
        services.AddSingleton(kernel);
        services.AddScoped(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
    }

    private static void AddWallabagBreefPublisher(IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddRefitClient<IWallabagAuthClient>();
        services.Configure<WallabagOptions>(configuration.GetSection("Wallabag"));

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
