using Elzik.Breef.Domain;
using Microsoft.Extensions.Logging;

namespace Elzik.Breef.Infrastructure.ContentExtractors
{
    public class ContentExtractorStrategy : IContentExtractor
    {
        private readonly ILogger<ContentExtractorStrategy> _logger;
        private readonly List<IContentExtractor> _extractors;

        public ContentExtractorStrategy(ILogger<ContentExtractorStrategy> logger, 
            IEnumerable<IContentExtractor> specificExtractors, IContentExtractor defaultExtractor)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(specificExtractors);
            ArgumentNullException.ThrowIfNull(defaultExtractor);

            _logger = logger;

            if (specificExtractors.Contains(defaultExtractor))
                throw new ArgumentException("Default extractor should not be in the specific extractors list.");

            _extractors = [.. specificExtractors, defaultExtractor];
        }

        public bool CanHandle(string webPageUrl) => true;

        public async Task<Extract> ExtractAsync(string webPageUrl)
        {
            var extractor = _extractors.First(e => e.CanHandle(webPageUrl));

            _logger.LogInformation("Extraction will be provided for by {ExtractorName}", extractor.GetType().Name);

            return await extractor.ExtractAsync(webPageUrl);
        }
    }

}
