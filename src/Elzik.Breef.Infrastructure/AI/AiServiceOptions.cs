using System.ComponentModel.DataAnnotations;

namespace Elzik.Breef.Infrastructure.AI;

public class AiServiceOptions
{
    [Required]
    public required AiServiceProviders Provider { get; set; }
    
    [Required]
    public required string ModelId { get; set; }

    [Required, Url]
    public required string EndpointUrl { get; set; }

    [Required]
    public required string ApiKey { get; set; }

    [Required, Range(1, int.MaxValue)]
    public required int Timeout { get; set; } = 100;
}
