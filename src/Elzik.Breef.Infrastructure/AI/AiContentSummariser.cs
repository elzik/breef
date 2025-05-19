using Elzik.Breef.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Elzik.Breef.Infrastructure.AI;

public class AiContentSummariser(
    ILogger<AiContentSummariser> logger, 
    IChatCompletionService Chat, 
    IOptions<AiContentSummariserOptions> summariserOptions) : IContentSummariser
{
    public async Task<string> SummariseAsync(string content)
    {
        var systemPrompt = @$"
You are an expert summarizer. Your task is to summarize the provided text:
  - Summarise text, including HTML entities.
  - Limit summaries to {summariserOptions.Value.TargetSummaryLengthPercentage}% of the original length but never more then {summariserOptions.Value.TargetSummaryMaxWordCount} words.
  - Ensure accurate attribution of information to the correct entities.
  - Do not include a link to the original articles.
  - Do not include teh title in the response.
  - Do not include any metadata n the response.
  - Do not include a code block in the response.";
        
        var formattingInstructions = "Summarise this content in an HTML format using paragraphs and " +
            "bullet points to enhance readability\n:";

        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddMessage(AuthorRole.User, $"{formattingInstructions}{content}");

        var result = await Chat.GetChatMessageContentAsync(chatHistory);

        if(result.Content is null)
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
