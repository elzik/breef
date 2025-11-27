using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure.ContentExtractors;

public record UntypedExtract(string Title, string Content, string OriginalUrl, string? PreviewImageUrl)
{
    public Extract WithType(string extractType) 
        => new(Title, Content, OriginalUrl, PreviewImageUrl, extractType);
}
