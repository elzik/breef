using System.ComponentModel.DataAnnotations;

namespace Elzik.Breef.Infrastructure.AI;

public class AiContentSummariserOptions
{
    [Required, Range(1, int.MaxValue)]
    public int TargetSummaryMaxWordCount { get; set; } = 200;

    [Required, Range(1, 100)]
    public double TargetSummaryLengthPercentage { get; set; } = 10;
}
