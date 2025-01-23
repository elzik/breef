using Refit;

namespace Elzik.Breef.Infrastructure.Wallabag;

public class TokenRequest
{
    [AliasAs("grant_type")]
    public string GrantType { get; } = "password";

    [AliasAs("client_id")]
    public required string ClientId { get; set; }

    [AliasAs("client_secret")]
    public required string ClientSecret { get; set; }

    [AliasAs("username")]
    public required string Username { get; set; }

    [AliasAs("password")]
    public required string Password { get; set; }
}
