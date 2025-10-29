using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure.ContentExtractors;

public abstract class ContentExtractorBase : IContentExtractor
{
    private const string RequiredSuffix = "Extractor";

    protected ContentExtractorBase()
    {
        var typeName = GetType().Name;
        if (!typeName.EndsWith(RequiredSuffix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Content extractor class '{typeName}' must end with '{RequiredSuffix}' suffix. " +
                $"This convention is required to derive the ExtractType for domain objects.");
        }
    }

    public abstract bool CanHandle(string webPageUrl);

    public async Task<Extract> ExtractAsync(string webPageUrl)
    {
        var result = await CreateUntypedExtractAsync(webPageUrl);
        var extractType = GetExtractType();

        return result.WithType(extractType);
    }

    protected abstract Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl);

    private string GetExtractType()
    {
        var typeName = GetType().Name;

        return typeName[..^RequiredSuffix.Length];
    }
}
