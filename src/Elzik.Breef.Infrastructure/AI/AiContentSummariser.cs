using Elzik.Breef.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Elzik.Breef.Infrastructure.AI;

public class AiContentSummariser(
    ILogger<AiContentSummariser> logger, 
    IChatCompletionService Chat) : IContentSummariser
{
    public async Task<string> SummariseAsync(string content, string instructions)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(instructions);
        var formattingInstructions = "Summarise this content in an HTML format using paragraphs and " +
            "bullet points to enhance readability\n:";

        var chatHistory = new ChatHistory(instructions);
        chatHistory.AddMessage(AuthorRole.User, $"{formattingInstructions}{content}");

        var result = await Chat.GetChatMessageContentAsync(chatHistory);
        
        if (string.IsNullOrWhiteSpace(result.Content))
        {
            throw new InvalidOperationException("Chat completion returned no result.");
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            var wordCount = result.Content.Split(' ').Length;
            var ratio = (double)result.Content.Length / content.Length;

            logger.LogInformation(
                "Summary generated in {WordCount} words, {Percentage} of original content.",
                wordCount, ratio.ToString("P1"));
        }

        return result.Content;
    }
}
