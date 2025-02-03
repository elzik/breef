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
            var content = await contentExtractor.ExtractAsync(url);
            var summary = await contentSummariser.SummariseAsync(content);

            var breef = new Domain.Breef(url, url ,summary);

            var publishedBreef = await breefPublisher.PublishAsync(breef);

            return publishedBreef;
        }
    }
}
