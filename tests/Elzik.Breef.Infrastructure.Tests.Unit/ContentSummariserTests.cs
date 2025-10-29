using Elzik.Breef.Infrastructure.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit;

public class ContentSummariserTests
{
    private readonly string _testSummary;
    private readonly FakeLogger<AiContentSummariser> _logger;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly AiContentSummariser _contentSummariser;
    private readonly string _testContent;
    private readonly ChatMessageContent? _testSummaryResult;

    public ContentSummariserTests()
    {
        _logger = new FakeLogger<AiContentSummariser>();
        _chatCompletionService = Substitute.For<IChatCompletionService>();
        _contentSummariser = new AiContentSummariser(_logger, _chatCompletionService);

        _testContent = "This is a test content.";
        _testSummary = "Test summary.";

        var testChatHistory = new ChatHistory("system prompt");
        testChatHistory.AddMessage(AuthorRole.Assistant, _testContent);
        _testSummaryResult = new ChatMessageContent(AuthorRole.Assistant, _testSummary);

        _ = _chatCompletionService.GetChatMessageContentsAsync(
            Arg.Is<ChatHistory>(ch => ch.Any(
                m => m.Content != null && 
                m.Content.EndsWith(_testContent) && 
                m.Role == AuthorRole.User)),
            Arg.Any<PromptExecutionSettings>(),
            Arg.Any<Kernel>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ChatMessageContent>>(
                [_testSummaryResult]));
    }

    [Fact]
    public async Task SummariseAsync_ValidContent_ReturnsSummary()
    {
        // Act
        var result = await _contentSummariser.SummariseAsync(_testContent, "Test instructions");

        // Assert
        result.ShouldBe("Test summary.");
    }

    [Fact]
    public async Task SummariseAsync_ValidContent_ProvidesModelInstructions()
    {
        // Arrange
        var instructions = "Test instructions";

        // Act
        _ = await _contentSummariser.SummariseAsync(_testContent, instructions);

        // Assert
        await _chatCompletionService.Received(1).GetChatMessageContentsAsync(
            Arg.Is<ChatHistory>(ch => ch.Any(m =>
                m.Content == instructions && m.Role == AuthorRole.System)),
            Arg.Any<PromptExecutionSettings>(),
            Arg.Any<Kernel>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SummariseAsync_ValidContent_Logs()
    {
        // Act
        await _contentSummariser.SummariseAsync(_testContent, "Test instructions");

        // Assert
        _logger.Collector.Count.ShouldBe(1);
        _logger.LatestRecord.Level.ShouldBe(LogLevel.Information);
        double ratio = 0.565;
        _logger.LatestRecord.Message.ShouldBe(
            string.Format("Summary generated in 2 words, {0} of original content.", 
            ratio.ToString("P1")));
    }
}
