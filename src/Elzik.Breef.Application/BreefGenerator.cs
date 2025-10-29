using Elzik.Breef.Domain;

namespace Elzik.Breef.Application
{
    public class BreefGenerator( 
        IContentExtractor contentExtractor,
        IContentSummariser contentSummariser,
        IBreefPublisher breefPublisher) : IBreefGenerator
    {
        public async Task<PublishedBreef> GenerateBreefAsync(string url)
        {
            var extract = await contentExtractor.ExtractAsync(url);

            var instructionsPath = Path.Combine(AppContext.BaseDirectory, "SummarisationInstructions", "HtmlContent.md");
            var instructions = await File.ReadAllTextAsync(instructionsPath);

            var summary = await contentSummariser.SummariseAsync(extract.Content, instructions);

            var breef = new Domain.Breef(url, extract.Title ,summary, extract.PreviewImageUrl);

            var publishedBreef = await breefPublisher.PublishAsync(breef);

            return publishedBreef;
        }
    }
}
