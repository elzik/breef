using Elzik.Breef.Api.Presentation;
using Elzik.Breef.Application;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

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
