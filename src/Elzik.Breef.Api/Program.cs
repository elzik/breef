using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Application;
using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure;
using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Refit;
using System.Net.Http.Headers;

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

        builder.Services.AddTransient<IWebPageDownloader, WebPageDownloader>();
        builder.Services.AddTransient<IContentExtractor, ContentExtractor>();
        builder.Services.AddTransient<IContentSummariser, ContentSummariser>();

        builder.Services.AddRefitClient<IWallabagAuthClient>();

        builder.Services.Configure<WallabagOptions>(options =>
        {
            options.BaseUrl = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_URL")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_URL must contain a Wallabag URL.");
            options.ClientId = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_CLIENT_ID")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_CLIENT_ID must contain a Wallabag client ID.");
            options.ClientSecret = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_CLIENT_SECRET")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_CLIENT_SECRET must contain a Wallabag client secret.");
            options.Username = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_USERNAME")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_USERNAME must contain a Wallabag username.");
            options.Password = Environment.GetEnvironmentVariable("BREEF_TESTS_WALLABAG_PASSWORD")
                ?? throw new InvalidOperationException("BREEF_TESTS_WALLABAG_PASSWORD must contain a Wallabag password.");
        });

        var wallabagOptions = builder.Services.BuildServiceProvider()
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
            }
        };

#if DEBUG
        refitSettings.HttpMessageHandlerFactory = () => new HttpMessageDebugLoggingHandler();
#endif

        builder.Services.AddRefitClient<IWallabagClient>(refitSettings)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(wallabagOptions.BaseUrl);
            });

        builder.Services.AddTransient<IBreefPublisher, WallabagBreefPublisher>();
        builder.Services.AddTransient<IBreefGenerator, BreefGenerator>();

        var modelId = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_MODEL_ID") 
            ?? throw new InvalidOperationException("BREEF_TESTS_AI_MODEL_ID must contain a model ID.");
        var endpoint = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_ENDPOINT")
            ?? throw new InvalidOperationException("BREEF_TESTS_AI_ENDPOINT must contain an endpoint URL.");
        var apiKey = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_API_KEY")
            ?? throw new InvalidOperationException("BREEF_TESTS_AI_API_KEY must contain an API key.");
        var kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
        kernelBuilder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        var kernel = kernelBuilder.Build();
        builder.Services.AddSingleton(kernel);
        builder.Services.AddScoped(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());



        var app = builder.Build();
        app.UseCors();
        app.UseAuth();

        app.AddBreefEndpoints();

        await app.RunAsync();
    }
}
