namespace Elzik.Breef.Domain
{
    public record Breef(
        string OrigUrl, 
        string Title, 
        string Content,
        string? PreviewImageUrl);
}
