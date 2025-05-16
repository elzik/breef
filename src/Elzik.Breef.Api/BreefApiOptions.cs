using System.ComponentModel.DataAnnotations;

namespace Elzik.Breef.Api;

public class BreefApiOptions
{
    [Required]
    public required string ApiKey { get; set; }
}
