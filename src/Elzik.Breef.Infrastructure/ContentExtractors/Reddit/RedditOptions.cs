using System.ComponentModel.DataAnnotations;
using System.Linq;

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
            .Where(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .Select(url => new Uri(url).Host);

    private IEnumerable<string> GetEffectiveAdditionalBaseAddresses()
    {
        if (AdditionalBaseAddresses.Count == 0)
        {
            return ["https://reddit.com"];
        }

        return AdditionalBaseAddresses;
    }
}