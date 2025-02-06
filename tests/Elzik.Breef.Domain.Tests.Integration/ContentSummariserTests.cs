using Elzik.Breef.Infrastructure;
using Elzik.Breef.Infrastructure.Wallabag;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Shouldly;
using Xunit.Abstractions;

namespace Elzik.Breef.Domain.Tests.Integration;

public partial class ContentSummariserTests(ITestOutputHelper TestOutputHelper)
{
    [Theory]
    [InlineData("TestHtmlPage-ExpectedContent.txt")]
    [InlineData("BbcNewsPage-ExpectedContent.txt")]
    public async Task Summarise_WithValidContent_ReturnsSummary(string testExtractedContentFile)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("breef_")
                .Build();
        var aiServiceOptions = configuration.GetRequiredSection("AiService").Get<AiServiceOptions>()
            ?? throw new InvalidOperationException("AI service options not found in AiService configuration section.");

        var testContent = await File.ReadAllTextAsync(Path.Join("../../../../TestData", testExtractedContentFile));

        var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
            aiServiceOptions.ModelId, aiServiceOptions.EndpointUrl, aiServiceOptions.ApiKey);
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddProvider(new TestOutputLoggerProvider(TestOutputHelper));
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
        });

        var kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Act
        var contentSummariser = new ContentSummariser(chatCompletionService);
        var summary = await contentSummariser.SummariseAsync(testContent);

        // Assert
        TestOutputHelper.WriteLine(summary);
        summary.Length.ShouldBeGreaterThan(100, "because this is the minimum acceptable summary length");
    }
}
