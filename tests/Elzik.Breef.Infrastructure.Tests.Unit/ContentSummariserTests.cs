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
        private readonly FakeLogger<ContentSummariser> _logger;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ContentSummariser _contentSummariser;

        public ContentSummariserTests()
        {
            _logger = new FakeLogger<ContentSummariser>();
            _chatCompletionService = Substitute.For<IChatCompletionService>();
            _contentSummariser = new ContentSummariser(_logger, _chatCompletionService);
        }

        [Fact]
        public async Task SummariseAsync_ValidContent_ReturnsSummary()
        {
            // Arrange
            var testContent = "This is a test content.";
            var testChatHistory = new ChatHistory("system prompt");
            testChatHistory.AddMessage(AuthorRole.Assistant, testContent);
            var testSummaryResult = new ChatMessageContent(AuthorRole.Assistant, "Test summary.");

            _chatCompletionService.GetChatMessageContentsAsync(
                Arg.Is<ChatHistory>(ch => ch.Any(m => 
                    m.Content == testContent && m.Role == AuthorRole.Assistant)), 
                Arg.Any<PromptExecutionSettings>(),
                Arg.Any<Kernel>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ChatMessageContent>>(
                    [testSummaryResult]));

            // Act
            var result = await _contentSummariser.SummariseAsync(testContent);

            // Assert
            result.ShouldBe("Test summary.");
        }

        [Fact]
        public async Task SummariseAsync_ValidContent_ProvidesModelInstructions()
        {
            // Arrange
            var testContent = "This is a test content.";
            var testChatHistory = new ChatHistory("system prompt");
            testChatHistory.AddMessage(AuthorRole.Assistant, testContent);
            var testSummaryResult = new ChatMessageContent(AuthorRole.Assistant, "Test summary.");

            _chatCompletionService.GetChatMessageContentsAsync(
                Arg.Is<ChatHistory>(ch => ch.Any(m =>
                    m.Content == testContent && m.Role == AuthorRole.Assistant)),
                Arg.Any<PromptExecutionSettings>(),
                Arg.Any<Kernel>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ChatMessageContent>>(
                    [testSummaryResult]));

            // Act
            var result = await _contentSummariser.SummariseAsync(testContent);

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
            // Arrange
            var testContent = "This is a test content.";
            var testChatHistory = new ChatHistory("system prompt");
            testChatHistory.AddMessage(AuthorRole.Assistant, testContent);
            var testSummaryResult = new ChatMessageContent(AuthorRole.Assistant, "Test summary.");

            _chatCompletionService.GetChatMessageContentsAsync(
                Arg.Any<ChatHistory>(),
                Arg.Any<PromptExecutionSettings>(),
                Arg.Any<Kernel>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<ChatMessageContent>>(
                    [testSummaryResult]));

            // Act
            await _contentSummariser.SummariseAsync(testContent);

            // Assert
            _logger.Collector.Count.ShouldBe(1);
            _logger.LatestRecord.Level.ShouldBe(LogLevel.Information);
            _logger.LatestRecord.Message.ShouldBe(
                "Summary generated in 2 words, 56.5% of original content.");
        }
    }
}
