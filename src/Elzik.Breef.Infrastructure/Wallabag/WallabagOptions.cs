namespace Elzik.Breef.Infrastructure.Wallabag
{
    public record WallabagOptions(
        string BaseUrl,
        string ClientId,
        string ClientSecret,
        string Username,
        string Password);
}
