using Elzik.Breef.Api.Auth;
using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Application;
using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure;
using Elzik.Breef.Infrastructure.AI;
using Elzik.Breef.Infrastructure.ContentExtractors;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw;
using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Options;
using Refit;
using Serilog;
using System.Reflection;

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
            .ReadFrom.Configuration(configuration)
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

        builder.Services.AddOptions<BreefApiOptions>()
            .Bind(configuration.GetSection("BreefApi"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddAuth();

        builder.Services.AddOptions<HttpClientOptions>()
            .Bind(configuration.GetSection("HttpClient"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddHttpClient("BreefDownloader")
            .ConfigureHttpClient((provider, client) =>
            {
                var httpClientOptions = provider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(httpClientOptions.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", httpClientOptions.UserAgent);
            });

        builder.Services.AddOptions<RedditOptions>()
            .Bind(configuration.GetSection("Reddit"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        builder.Services.AddRefitClient<IRawRedditPostClient>()
            .ConfigureHttpClient((provider, client) =>
            {
                var redditOptions = provider.GetRequiredService<IOptions<RedditOptions>>().Value;
                client.BaseAddress = new Uri(redditOptions.DefaultBaseAddress);
            });

        builder.Services.AddRefitClient<IRawSubredditClient>()
            .ConfigureHttpClient((provider, client) =>
            {
                var redditOptions = provider.GetRequiredService<IOptions<RedditOptions>>().Value;
                client.BaseAddress = new Uri(redditOptions.DefaultBaseAddress);
            });

        builder.Services.AddTransient<IRawRedditPostTransformer, RawRedditPostTransformer>();
        builder.Services.AddTransient<IRedditPostClient, RedditPostClient>();
        builder.Services.AddTransient<IRawNewInSubredditTransformer, RawNewInSubredditTransformer>();
        builder.Services.AddTransient<ISubredditClient, SubredditClient>();

        builder.Services.AddTransient<HtmlContentExtractor>();
        builder.Services.AddTransient<SubredditContentExtractor>();
        builder.Services.AddTransient<RedditPostContentExtractor>();
        builder.Services.AddTransient<ISubredditImageExtractor, SubredditContentExtractor>();
        builder.Services.AddTransient<IContentExtractor>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ContentExtractorStrategy>>();
            var defaultContentExtractor = provider.GetRequiredService<HtmlContentExtractor>();
            var subredditExtractor = provider.GetRequiredService<SubredditContentExtractor>();
            var redditPostExtractor = provider.GetRequiredService<RedditPostContentExtractor>();
            return new ContentExtractorStrategy(logger, 
                [subredditExtractor, redditPostExtractor], 
                defaultContentExtractor);
        });

        builder.Services.AddOptions<AiServiceOptions>()
            .Bind(configuration.GetSection("AiService"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddOptions<AiContentSummariserOptions>()
            .Bind(configuration.GetSection("AiContentSummariser"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddAiContentSummariser();

        builder.Services.AddOptions<WallabagOptions>()
            .Bind(configuration.GetSection("Wallabag"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddWallabagBreefPublisher();

        builder.Services.AddTransient<IBreefGenerator, BreefGenerator>();

        var app = builder.Build();
        app.UseCors();
        app.UseAuth();

        app.AddBreefEndpoints();

        await app.RunAsync();
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
