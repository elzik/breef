using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Elzik.Breef.Api.Tests.Integration;

public class BreefApiOptionsTests
{
    [Fact]
    public void WhenValidated_MissingApiKey_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<BreefApiOptions>()
            .ValidateDataAnnotations();

        // Act
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<BreefApiOptions>>();
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldBe("DataAnnotation validation failed for 'BreefApiOptions' members: " +
            "'ApiKey' with the error: 'The ApiKey field is required.'.");
    }
}