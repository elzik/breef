using Elzik.Breef.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Elzik.Breef.Infrastructure
{
    public class ContentSummariser(ILogger<ContentSummariser> logger, IChatCompletionService Chat) : IContentSummariser
    {
        public async Task<string> SummariseAsync(string content)
        {
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

            var chatHistory = new ChatHistory(systemPrompt);
            chatHistory.AddMessage(AuthorRole.Assistant, content);

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
}
