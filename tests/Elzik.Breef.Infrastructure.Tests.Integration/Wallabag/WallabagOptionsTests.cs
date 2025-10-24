using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Elzik.Breef.Infrastructure.Wallabag;

namespace Elzik.Breef.Infrastructure.Tests.Integration.Wallabag;

public class WallabagOptionsTests
{
    [Fact]
    public void WhenValidated_MissingClientId_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WallabagOptions>()
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WallabagOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'ClientId' with the error: 'The ClientId field is required.'");
    }

    [Fact]
    public void WhenValidated_MissingClientSecret_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WallabagOptions>()
            .Configure(o =>
            {
                o.ClientId = "client-id";
                o.ClientSecret = string.Empty;
                o.BaseUrl = "https://wallabag.example.com";
                o.Username = "user";
                o.Password = "pass";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WallabagOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'ClientSecret' with the error: 'The ClientSecret field is required.'");
    }

    [Fact]
    public void WhenValidated_MissingBaseUrl_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WallabagOptions>()
            .Configure(o =>
            {
                o.ClientId = "client-id";
                o.ClientSecret = "secret";
                o.BaseUrl = string.Empty;
                o.Username = "user";
                o.Password = "pass";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WallabagOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'BaseUrl' with the error: 'The BaseUrl field is required.'");
    }

    [Fact]
    public void WhenValidated_InvalidBaseUrl_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WallabagOptions>()
            .Configure(o =>
            {
                o.ClientId = "client-id";
                o.ClientSecret = "secret";
                o.BaseUrl = "not-a-url";
                o.Username = "user";
                o.Password = "pass";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WallabagOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'BaseUrl' with the error: 'The BaseUrl field is not a valid fully-qualified http, https, or ftp URL.'");
    }

    [Fact]
    public void WhenValidated_MissingUsername_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WallabagOptions>()
            .Configure(o =>
            {
                o.ClientId = "client-id";
                o.ClientSecret = "secret";
                o.BaseUrl = "https://wallabag.example.com";
                o.Username = string.Empty;
                o.Password = "pass";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WallabagOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'Username' with the error: 'The Username field is required.'");
    }

    [Fact]
    public void WhenValidated_MissingPassword_ShouldFailValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WallabagOptions>()
            .Configure(o =>
            {
                o.ClientId = "client-id";
                o.ClientSecret = "secret";
                o.BaseUrl = "https://wallabag.example.com";
                o.Username = "user";
                o.Password = string.Empty;
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WallabagOptions>>();

        // Act
        var ex = Assert.Throws<OptionsValidationException>(() => options.Value);

        // Assert
        ex.Message.ShouldContain("'Password' with the error: 'The Password field is required.'");
    }
    [Fact]
    public void WhenValidated_WithValidOptions_ShouldPassValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<WallabagOptions>()
            .Configure(o =>
            {
                o.ClientId = "client-id";
                o.ClientSecret = "secret";
                o.BaseUrl = "https://wallabag.example.com";
                o.Username = "user";
                o.Password = "pass";
            })
            .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WallabagOptions>>();

        // Act
        var value = options.Value;

        // Assert
        value.ShouldNotBeNull();
        value.ClientId.ShouldBe("client-id");
        value.ClientSecret.ShouldBe("secret");
        value.BaseUrl.ShouldBe("https://wallabag.example.com");
        value.Username.ShouldBe("user");
        value.Password.ShouldBe("pass");
    }
}
