using AspNetCore.Authentication.ApiKey;
using System.Security.Claims;

namespace Elzik.Breef.Api.Auth;

public class ApiKey(string key) : IApiKey
{
    public string Key { get; } = key;

    public string OwnerName { get; } = "DefaultOwner";

    public IReadOnlyCollection<Claim> Claims => [];
}
