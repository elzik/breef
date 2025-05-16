using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Elzik.Breef.Infrastructure.AI;
using Xunit;

namespace Elzik.Breef.Infrastructure.Tests.Integration;

public class AiServiceOptionsTests
{
    [Fact]
    public void WhenValidated_MissingProvider_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiServiceOptions>()
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiServiceOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("DataAnnotation validation failed for 'AiServiceOptions' members: " +
            "'ModelId' with the error: 'The ModelId field is required.'.");
    }

    [Fact]
    public void WhenValidated_MissingModelId_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiServiceOptions>()
            .Configure(o =>
            {
                o.Provider = AiServiceProviders.OpenAI;
                o.ModelId = string.Empty;
                o.EndpointUrl = "https://api.example.com";
                o.ApiKey = "key";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiServiceOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'ModelId' with the error: 'The ModelId field is required.'");
    }

    [Fact]
    public void WhenValidated_MissingEndpointUrl_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiServiceOptions>()
            .Configure(o =>
            {
                o.Provider = AiServiceProviders.OpenAI;
                o.ModelId = "gpt-4";
                o.EndpointUrl = string.Empty;
                o.ApiKey = "key";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiServiceOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'EndpointUrl' with the error: 'The EndpointUrl field is required.'");
    }

    [Fact]
    public void WhenValidated_InvalidEndpointUrl_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiServiceOptions>()
            .Configure(o =>
            {
                o.Provider = AiServiceProviders.OpenAI;
                o.ModelId = "gpt-4";
                o.EndpointUrl = "not-a-url";
                o.ApiKey = "key";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiServiceOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'EndpointUrl' with the error: 'The EndpointUrl field is not a valid fully-qualified http, https, or ftp URL.'");
    }

    [Fact]
    public void WhenValidated_MissingApiKey_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<AiServiceOptions>()
            .Configure(o =>
            {
                o.Provider = AiServiceProviders.OpenAI;
                o.ModelId = "gpt-4";
                o.EndpointUrl = "https://api.example.com";
                o.ApiKey = string.Empty;
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AiServiceOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'ApiKey' with the error: 'The ApiKey field is required.'");
    }
}
