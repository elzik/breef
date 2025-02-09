using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit
{
    public class ContentSummariserTests
    {
        private readonly string _testSummary;
        private readonly FakeLogger<ContentSummariser> _logger;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ContentSummariser _contentSummariser;
        private readonly string _testContent;
        private readonly ChatMessageContent? _testSummaryResult;

        public ContentSummariserTests()
        {
            _logger = new FakeLogger<ContentSummariser>();
            _chatCompletionService = Substitute.For<IChatCompletionService>();
            _contentSummariser = new ContentSummariser(_logger, _chatCompletionService);

            _testContent = "This is a test content.";
            _testSummary = "Test summary.";

            var testChatHistory = new ChatHistory("system prompt");
            testChatHistory.AddMessage(AuthorRole.Assistant, _testContent);
            _testSummaryResult = new ChatMessageContent(AuthorRole.Assistant, _testSummary);

            _chatCompletionService.GetChatMessageContentsAsync(
                Arg.Is<ChatHistory>(ch => ch.Any(m =>
                    m.Content == _testContent && m.Role == AuthorRole.Assistant)),
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
            var result = await _contentSummariser.SummariseAsync(_testContent);

            // Assert
            result.ShouldBe("Test summary.");
        }

        [Fact]
        public async Task SummariseAsync_ValidContent_ProvidesModelInstructions()
        {
            // Act
            var result = await _contentSummariser.SummariseAsync(_testContent);

            // Assert
            var systemPrompt = @"
1. Summary Goal:
    - Summarise text, including HTML entities.
    - Limit summaries to 10% of the original length but never more then 200 words.
    - Ensure accurate attribution of information to the correct entities.
    - Do not include a link to the original articles.
2. Formatting:
    - Utilize HTML text formatting, such as:
        - Paragraphs
        - Bullet points
    - Aim to enhance readability.";
            await _chatCompletionService.Received(1).GetChatMessageContentsAsync(
                Arg.Is<ChatHistory>(ch => ch.Any(m =>
                    m.Content == systemPrompt && m.Role == AuthorRole.System)),
                Arg.Any<PromptExecutionSettings>(),
                Arg.Any<Kernel>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SummariseAsync_ValidContent_Logs()
        {
            // Act
            await _contentSummariser.SummariseAsync(_testContent);

            // Assert
            _logger.Collector.Count.ShouldBe(1);
            _logger.LatestRecord.Level.ShouldBe(LogLevel.Information);
            double ratio = 0.565;
            _logger.LatestRecord.Message.ShouldBe(
                $"Summary generated in 2 words, {ratio.ToString("P1")} of original content.");
        }
    }
}
