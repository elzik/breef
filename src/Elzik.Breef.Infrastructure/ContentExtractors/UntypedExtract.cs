using Elzik.Breef.Domain;

namespace Elzik.Breef.Infrastructure.ContentExtractors;

public record UntypedExtract(string Title, string Content, string? PreviewImageUrl)
{
    public Extract WithType(string extractType) 
        => new(Title, Content, PreviewImageUrl, extractType);
}
