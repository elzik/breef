namespace Elzik.Breef.Infrastructure.AI;

public class AiContentSummariserOptions
{
    public int TargetSummaryMaxWordCount { get; set; } = 200;

    public double TargetSummaryLengthPercentage { get; set; } = 10;
}
