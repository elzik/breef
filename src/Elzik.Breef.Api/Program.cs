using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Application;
using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure;
using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Refit;
using System.Text.Json.Serialization;
using System.Text.Json;

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

        builder.Logging.AddFilter("Default", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

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

        var configuration = builder.Configuration;

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
        services.PostConfigure<AiServiceOptions>(aiServiceOptions =>
        {
            if (string.IsNullOrWhiteSpace(aiServiceOptions.ModelId))
            {
                throw new InvalidOperationException(
                    "The AI model ID must be specified in the appsettings.json file in AiService.ModelId");
            }

            if (string.IsNullOrWhiteSpace(aiServiceOptions.EndpointUrl))
            {
                throw new InvalidOperationException(
                    "The AI service endpoint URL must be specified in the appsettings.json file in AiService.EndpointUrl");
            }

            var aiServiceApiKey = Environment.GetEnvironmentVariable("BREEF_AI_API_KEY");
            if (string.IsNullOrWhiteSpace(aiServiceOptions.ApiKey)
                && string.IsNullOrWhiteSpace(aiServiceApiKey))
            {
                throw new InvalidOperationException(
                    "The AI service API key must be specified once in the BREEF_AI_API_KEY environment " +
                    "variable (recommended) or the appsettings.json file in AiService.ApiKey");
            }
            if (!string.IsNullOrWhiteSpace(aiServiceOptions.ApiKey)
                && !string.IsNullOrWhiteSpace(aiServiceApiKey))
            {
                throw new InvalidOperationException(
                    "The AI service API key has been defined twice. It must be specified in the BREEF_AI_API_KEY environment " +
                    "variable (recommended) or the appsettings.json file in AiService.ApiKey but not both.");
            }
            if (!string.IsNullOrWhiteSpace(aiServiceApiKey))
            {
                aiServiceOptions.ApiKey = aiServiceApiKey;
            }
        });

        var aiServiceOptions = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AiServiceOptions>>().Value;


        var kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
            aiServiceOptions.ModelId, aiServiceOptions.EndpointUrl, aiServiceOptions.ApiKey);
        kernelBuilder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        var kernel = kernelBuilder.Build();
        services.AddSingleton(kernel);
        services.AddScoped(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
    }

    private static void AddWallabagBreefPublisher(IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddRefitClient<IWallabagAuthClient>();
        services.Configure<WallabagOptions>(configuration.GetSection("Wallabag"));
        services.PostConfigure<WallabagOptions>(wallabagOptions =>
        {
            if (string.IsNullOrWhiteSpace(wallabagOptions.BaseUrl))
            {
                throw new InvalidOperationException(
                    "The Wallabag base URL must be specified in the appsettings.json file in Wallabag.BaseUrl");
            }

            if (string.IsNullOrWhiteSpace(wallabagOptions.ClientId))
            {
                throw new InvalidOperationException(
                    "The Wallabag client ID must be specified in the appsettings.json file in Wallabag.ClientId");
            }

            var wallabagClientSecretEnv = Environment.GetEnvironmentVariable("BREEF_WALLABAG_CLIENT_SECRET");
            if (string.IsNullOrWhiteSpace(wallabagOptions.ClientSecret)
                && string.IsNullOrWhiteSpace(wallabagClientSecretEnv))
            {
                throw new InvalidOperationException(
                    "The Wallabag client secret must be specified once in the BREEF_WALLABAG_CLIENT_SECRET environment " +
                    "variable (recommended) or the appsettings.json file in Wallabag.ClientSecret");
            }
            if (!string.IsNullOrWhiteSpace(wallabagOptions.ClientSecret)
                && !string.IsNullOrWhiteSpace(wallabagClientSecretEnv))
            {
                throw new InvalidOperationException(
                    "The Wallabag client secret has been defined twice. It must be specified in the BREEF_WALLABAG_CLIENT_SECRET environment " +
                    "variable (recommended) or the appsettings.json file in Wallabag.ClientSecret but not both.");
            }
            if (!string.IsNullOrWhiteSpace(wallabagClientSecretEnv))
            {
                wallabagOptions.ClientSecret = wallabagClientSecretEnv;
            }

            if (string.IsNullOrWhiteSpace(wallabagOptions.Username))
            {
                throw new InvalidOperationException(
                    "The Wallabag username must be specified in the appsettings.json file in Wallabag.Username");
            }

            var wallabagPasswordEnv = Environment.GetEnvironmentVariable("BREEF_WALLABAG_PASSWORD");
            if (string.IsNullOrWhiteSpace(wallabagOptions.Password)
                && string.IsNullOrWhiteSpace(wallabagPasswordEnv))
            {
                throw new InvalidOperationException(
                    "The Wallabag password must be specified once in the BREEF_WALLABAG_PASSWORD environment " +
                    "variable (recommended) or the appsettings.json file in Wallabag.Password");
            }
            if (!string.IsNullOrWhiteSpace(wallabagOptions.Password)
                && !string.IsNullOrWhiteSpace(wallabagPasswordEnv))
            {
                throw new InvalidOperationException(
                    "The Wallabag password has been defined twice. It must be specified in the BREEF_WALLABAG_PASSWORD environment " +
                    "variable (recommended) or the appsettings.json file in Wallabag.Password but not both");
            }
            if (!string.IsNullOrWhiteSpace(wallabagPasswordEnv))
            {
                wallabagOptions.Password = wallabagPasswordEnv;
            }
        });

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
