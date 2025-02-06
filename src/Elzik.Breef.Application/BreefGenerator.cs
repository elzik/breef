using Elzik.Breef.Domain;
using System.Diagnostics;

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
            var summary = await contentSummariser.SummariseAsync(extract.Content);

            var breef = new Domain.Breef(url, extract.Title ,summary, extract.PreviewImageUrl);

            var publishedBreef = await breefPublisher.PublishAsync(breef);

            return publishedBreef;
        }
    }
}
