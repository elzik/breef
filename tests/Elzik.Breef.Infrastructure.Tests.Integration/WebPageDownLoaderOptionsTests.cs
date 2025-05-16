using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration;

public class WebPageDownLoaderOptionsTests
{
    [Fact]
    public void WhenValidated_MissingUserAgent_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WebPageDownLoaderOptions>()
            .Configure(o => o.UserAgent = string.Empty)
            .ValidateDataAnnotations();
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WebPageDownLoaderOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldBe("DataAnnotation validation failed for 'WebPageDownLoaderOptions' members: " +
            "'UserAgent' with the error: 'The UserAgent field is required.'.");

    }
}
