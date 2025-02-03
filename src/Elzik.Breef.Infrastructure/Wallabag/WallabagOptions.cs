namespace Elzik.Breef.Infrastructure.Wallabag
{
    public class WallabagOptions
    {
        public required string BaseUrl { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
