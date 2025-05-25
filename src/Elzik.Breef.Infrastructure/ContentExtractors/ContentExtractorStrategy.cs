using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure.ContentExtractors
{
    public class ContentExtractorStrategy : IContentExtractor
    {
        private readonly List<IContentExtractor> _extractors;

        public ContentExtractorStrategy(IEnumerable<IContentExtractor> specificExtractors, IContentExtractor defaultExtractor)
        {
            ArgumentNullException.ThrowIfNull(specificExtractors);
            ArgumentNullException.ThrowIfNull(defaultExtractor);

            if (specificExtractors.Contains(defaultExtractor))
                throw new ArgumentException("Default extractor should not be in the specific extractors list.");

            _extractors = [.. specificExtractors, defaultExtractor];
        }

        public bool CanHandle(string webPageUrl) => true;

        public async Task<Extract> ExtractAsync(string webPageUrl)
        {
            var extractor = _extractors.First(e => e.CanHandle(webPageUrl));
            return await extractor.ExtractAsync(webPageUrl);
        }
    }

}
