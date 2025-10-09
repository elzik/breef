using System.ComponentModel.DataAnnotations;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit;

public class RedditOptions
{
    [Required]
    [Url]
    public string DefaultBaseAddress { get; set; } = "https://www.reddit.com";

    public List<string> AdditionalBaseAddresses { get; set; } = [];

    public IEnumerable<string> AllBaseAddresses =>
        new[] { DefaultBaseAddress }.Concat(GetEffectiveAdditionalBaseAddresses());

    public IEnumerable<string> AllDomains =>
        AllBaseAddresses
            .Select(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null)
            .Where(uri => uri != null)
            .Select(uri => uri!.Host);

    private List<string> GetEffectiveAdditionalBaseAddresses()
    {
        if (AdditionalBaseAddresses.Count == 0)
        {
            return ["https://reddit.com"];
        }

        return AdditionalBaseAddresses;
    }
}