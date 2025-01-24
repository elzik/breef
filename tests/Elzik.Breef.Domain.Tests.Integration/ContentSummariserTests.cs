using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;

namespace Elzik.Breef.Domain.Tests.Integration
{
    public partial class ContentSummariserTests(ITestOutputHelper TestOutputHelper)
    {
        [SkippableTheory]
        [InlineData("TestHtmlPage-ExpectedContent.txt")]
        [InlineData("BbcNewsPage-ExpectedContent.txt")]
        public async Task Summarise_WithValidContent_ReturnsSummary(string testExtractedContentFile)
        {
            // Arrange
            var modelId = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_MODEL_ID");
            Skip.If(string.IsNullOrWhiteSpace(modelId), 
                "Skipped because no AI model ID provided in BREEF_TESTS_AI_MODEL_ID environment variable.");
            var endpoint = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_ENDPOINT");
            Skip.If(string.IsNullOrWhiteSpace(endpoint),
                "Skipped because no AI endpoint provided in BREEF_TESTS_AI_ENDPOINT environment variable.");
            var apiKey = Environment.GetEnvironmentVariable("BREEF_TESTS_AI_API_KEY");
            Skip.If(string.IsNullOrWhiteSpace(apiKey),
                "Skipped because no AI API key provided in BREEF_TESTS_AI_API_KEY environment variable.");

            var testContent = await File.ReadAllTextAsync(Path.Join("../../../../TestData", testExtractedContentFile));

            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddProvider(new TestOutputLoggerProvider(TestOutputHelper));
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            });

            var kernel = builder.Build();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Act
            var contentSummariser = new ContentSummariser(chatCompletionService);
            var summary = await contentSummariser.Summarise(testContent);

            // Assert
            TestOutputHelper.WriteLine(summary);
            summary.Length.Should().BeGreaterThan(100, "because this is the minimum acceptable summary length");
        }
    }
}
