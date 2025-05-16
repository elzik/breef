using Elzik.Breef.Infrastructure.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration;

public class AiContentSummariserOptionsTests
{
    [Fact]
    public void WhenValidated_InvalidTargetSummaryMaxWordCount_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiContentSummariserOptions>()
            .Configure(o =>
            {
                o.TargetSummaryMaxWordCount = 0;
                o.TargetSummaryLengthPercentage = 10;
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiContentSummariserOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'TargetSummaryMaxWordCount' with the error: 'The field TargetSummaryMaxWordCount " +
            "must be between 1 and 2147483647");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void WhenValidated_InvalidTargetSummaryLengthPercentage_ShouldFailValidation(int testTargetSummaryLengthPercentage)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiContentSummariserOptions>()
            .Configure(o =>
            {
                o.TargetSummaryMaxWordCount = 200;
                o.TargetSummaryLengthPercentage = testTargetSummaryLengthPercentage;
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiContentSummariserOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'TargetSummaryLengthPercentage' with the error: " +
            "'The field TargetSummaryLengthPercentage must be between 1 and 100.'");
    }

    [Fact]
    public void WhenValidated_ValidOptions_ShouldPassValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiContentSummariserOptions>()
            .Configure(o =>
            {
                o.TargetSummaryMaxWordCount = 150;
                o.TargetSummaryLengthPercentage = 25;
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiContentSummariserOptions>>();

        // Act
        var value = options.Value;

        // Assert
        value.TargetSummaryMaxWordCount.ShouldBe(150);
        value.TargetSummaryLengthPercentage.ShouldBe(25);
    }
}