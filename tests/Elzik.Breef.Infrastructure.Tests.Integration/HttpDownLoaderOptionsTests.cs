using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration;

public class HttpDownloaderOptionsTests
{
    [Fact]
    public void WhenValidated_MissingUserAgent_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<HttpDownloaderOptions>()
            .Configure(o => o.UserAgent = string.Empty)
            .ValidateDataAnnotations();
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<HttpDownloaderOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldBe("DataAnnotation validation failed for 'HttpDownloaderOptions' members: " +
            "'UserAgent' with the error: 'The UserAgent field is required.'.");
    }
    [Fact]
    public void WhenValidated_WithValidUserAgent_ShouldPassValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<HttpDownloaderOptions>()
            .Configure(o => o.UserAgent = "TestAgent/1.0")
            .ValidateDataAnnotations();
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<HttpDownloaderOptions>>();

        // Act
        var value = options.Value;

        // Assert
        value.UserAgent.ShouldBe("TestAgent/1.0");
    }
}
