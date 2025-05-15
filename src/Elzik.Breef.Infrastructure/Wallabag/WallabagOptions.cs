using System.ComponentModel.DataAnnotations;

namespace Elzik.Breef.Infrastructure.Wallabag;

public class WallabagOptions
{
    [Required, Url]
    public required string BaseUrl { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
