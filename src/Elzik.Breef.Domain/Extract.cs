namespace Elzik.Breef.Domain;

public record Extract(
    string Title, 
    string Content,
    string OriginalUrl, 
    string? PreviewImageUrl, 
    string ExtractType);
