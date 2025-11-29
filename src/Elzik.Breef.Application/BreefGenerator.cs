using Elzik.Breef.Domain;

namespace Elzik.Breef.Application
{
    public class BreefGenerator(
        IContentExtractor contentExtractor,
   IContentSummariser contentSummariser,
        IContentSummarisationInstructionProvider instructionProvider,
        IBreefPublisher breefPublisher) : IBreefGenerator
    {
        public async Task<PublishedBreef> GenerateBreefAsync(string url)
        {
            var extract = await contentExtractor.ExtractAsync(url);

            var instructions = instructionProvider.GetInstructions(extract.ExtractType);

            var summary = await contentSummariser.SummariseAsync(extract.Content, instructions);

            var breef = new Domain.Breef(extract.OriginalUrl, extract.Title, summary, extract.PreviewImageUrl);

            var publishedBreef = await breefPublisher.PublishAsync(breef);

            return publishedBreef;
        }
    }
}
