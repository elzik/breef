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
        services.AddSingleton<Kernel>(sp =>
        {
            var aiServiceOptions = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
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

            // Ideally, logging configuration should be centralized in the service layer.
            // However, due to SemanticKernel’s separate DI container, this is currently required here for logging support.
            // Without it SemanticKernel does not appear to log at all.
            kernelBuilder.Services.AddSerilog();

            return kernelBuilder.Build();
        });

        services.AddScoped(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

        services.AddTransient<IContentSummariser, AiContentSummariser>();
    }
}
