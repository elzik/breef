using Elzik.Breef.Infrastructure.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Shouldly;
using Xunit.Abstractions;

namespace Elzik.Breef.Infrastructure.Tests.Integration;

public class AiContentSummariserTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper
            ?? throw new ArgumentNullException(nameof(testOutputHelper));
    private readonly FakeLogger<AiContentSummariser> _fakeLogger = new();

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
            loggingBuilder.AddProvider(new TestOutputLoggerProvider(_testOutputHelper));
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
        });

        var kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var summariserOptions = Options.Create(new AiContentSummariserOptions
        {
            TargetSummaryLengthPercentage = 10,
            TargetSummaryMaxWordCount = 200
        });

        // Act
        var contentSummariser = new AiContentSummariser(_fakeLogger, chatCompletionService, summariserOptions);
        var summary = await contentSummariser.SummariseAsync(testContent);

        // Assert
        _testOutputHelper.WriteLine(summary);
        summary.Length.ShouldBeGreaterThan(100, "because this is the minimum acceptable summary length");
    }
}
