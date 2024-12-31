using AspNetCore.Authentication.ApiKey;
using System.Security.Claims;

namespace Elzik.Breef.Api;

public class ApiKey : IApiKey
{
    public ApiKey(string key)
    {
        Key = key;
        OwnerName = "DefaultOwner";
    }

    public string Key { get; }

    public string OwnerName { get; }

    public IReadOnlyCollection<Claim> Claims => [];
}
