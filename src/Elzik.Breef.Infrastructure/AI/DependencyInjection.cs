using Elzik.Breef.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Serilog;

namespace Elzik.Breef.Infrastructure.AI;

public static class DependencyInjection
{
    public static void AddAiContentSummariser(this IServiceCollection services)
    {
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
        services.AddTransient<IContentSummariser, AiContentSummariser>();
    }
}
