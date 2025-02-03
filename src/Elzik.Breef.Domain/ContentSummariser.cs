using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Elzik.Breef.Domain
{
    public class ContentSummariser(IChatCompletionService Chat) : IContentSummariser
    {
        public async Task<string> SummariseAsync(string content)
        {
            var prompt = $"Summarise the following content: {content}";

            var result = await Chat.GetChatMessageContentAsync(prompt);

            if(result.Content is null)
            {
                throw new InvalidOperationException("Chat completion returned no result.");
            }

            return result.Content;
        }
    }
}
