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
                    Kernel.CreateBuilder().AddOpenAIChatCompletion(
                        aiServiceOptions.ModelId, new Uri(aiServiceOptions.EndpointUrl), aiServiceOptions.ApiKey),
                AiServiceProviders.AzureOpenAI =>
                    Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
                        aiServiceOptions.ModelId, aiServiceOptions.EndpointUrl, aiServiceOptions.ApiKey),
                AiServiceProviders.Ollama =>
                    AddPreviewOllamaAIChatCompletion(aiServiceOptions),
                AiServiceProviders.NotSet =>
                    throw new InvalidOperationException("AiService provider is not set."),
                _ =>
                    throw new InvalidOperationException($"Unsupported AiService provider: {aiServiceOptions.Provider}"),
            };

            static IKernelBuilder AddPreviewOllamaAIChatCompletion(AiServiceOptions aiServiceOptions)
            {
                Log.Warning("Ollama provider is for evaluation purposes only and is subject to change or removal in future updates.");
                if(aiServiceOptions.ApiKey != null)
                {
                    Log.Warning("Ollama provider does not support API keys. A key has been supplied but it will not be used.");
                }
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(aiServiceOptions.EndpointUrl),
                    Timeout = TimeSpan.FromSeconds(600)
                };
#pragma warning disable SKEXP0070
                return Kernel.CreateBuilder().AddOllamaChatCompletion(
                    aiServiceOptions.ModelId, httpClient);
#pragma warning restore SKEXP0070
            }

            // Since SemanticKernel has its own DI container, logging must be added separately.
            // Adding the same Serilog logger registered for the rest of the application means configuration is only made in one place.
            kernelBuilder.Services.AddSerilog(Log.Logger);

            return kernelBuilder.Build();
        });

        services.AddScoped(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

        services.AddTransient<IContentSummariser, AiContentSummariser>();
    }
}
