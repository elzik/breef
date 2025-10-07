using Elzik.Breef.Infrastructure.ContentExtractors.Reddit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors.Reddit;

public class RedditOptionsTests
{
    [Fact]
    public void RedditOptions_DefaultBaseAddress_ShouldBeRedditCom()
    {
        // Arrange & Act
        var options = new RedditOptions();

        // Assert
        options.DefaultBaseAddress.ShouldBe("https://www.reddit.com");
    }

    [Fact]
    public void RedditOptions_AdditionalBaseAddresses_ShouldBeEmptyByDefault()
    {
        // Arrange & Act
        var options = new RedditOptions();

        // Assert
        options.AdditionalBaseAddresses.ShouldBeEmpty();
    }

    [Fact]
    public void RedditOptions_AllBaseAddresses_ShouldIncludeDefaultAndAdditional()
    {
        // Arrange
        var options = new RedditOptions
        {
            DefaultBaseAddress = "https://www.reddit.com",
            AdditionalBaseAddresses = ["https://custom.reddit.com", "https://alt.reddit.instance.com"]
        };

        // Act
        var allAddresses = options.AllBaseAddresses.ToList();

        // Assert
        allAddresses.ShouldBe(["https://www.reddit.com", "https://custom.reddit.com", "https://alt.reddit.instance.com"]);
    }

    [Fact]
    public void RedditOptions_AllDomains_ShouldExtractDomainsFromValidUrls()
    {
        // Arrange
        var options = new RedditOptions
        {
            DefaultBaseAddress = "https://www.reddit.com",
            AdditionalBaseAddresses = ["https://custom.reddit.com", "https://alt.reddit.instance.com"]
        };

        // Act
        var allDomains = options.AllDomains.ToList();

        // Assert
        allDomains.ShouldBe(["www.reddit.com", "custom.reddit.com", "alt.reddit.instance.com"]);
    }

    [Fact]
    public void RedditOptions_DefaultConfiguration_ShouldIncludeBothWwwAndNonWwwReddit()
    {
        // Arrange & Act
        var options = new RedditOptions();
        var allDomains = options.AllDomains.ToList();

        // Assert
        allDomains.ShouldBe(["www.reddit.com", "reddit.com"]);
    }

    [Fact]
    public void RedditOptions_AllDomains_ShouldSkipInvalidUrls()
    {
        // Arrange
        var options = new RedditOptions
        {
            DefaultBaseAddress = "https://www.reddit.com",
            AdditionalBaseAddresses = ["https://custom.reddit.com", "not-a-valid-url", "https://alt.reddit.instance.com"]
        };

        // Act
        var allDomains = options.AllDomains.ToList();

        // Assert
        allDomains.ShouldBe(["www.reddit.com", "custom.reddit.com", "alt.reddit.instance.com"]);
    }

    [Fact]
    public void RedditOptions_ConfigurationBinding_ShouldOverrideDefault()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>
        {
            { "Reddit:DefaultBaseAddress", "https://custom.reddit.com" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<RedditOptions>()
            .Bind(configuration.GetSection("Reddit"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var redditOptions = serviceProvider.GetRequiredService<IOptions<RedditOptions>>().Value;

        // Assert
        redditOptions.DefaultBaseAddress.ShouldBe("https://custom.reddit.com");
    }

    [Fact]
    public void RedditOptions_ConfigurationBinding_ShouldBindAdditionalBaseAddresses()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>
        {
            { "Reddit:DefaultBaseAddress", "https://www.reddit.com" },
            { "Reddit:AdditionalBaseAddresses:0", "https://custom.reddit.com" },
            { "Reddit:AdditionalBaseAddresses:1", "https://alt.reddit.instance.com" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<RedditOptions>()
            .Bind(configuration.GetSection("Reddit"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var redditOptions = serviceProvider.GetRequiredService<IOptions<RedditOptions>>().Value;

        // Assert
        redditOptions.DefaultBaseAddress.ShouldBe("https://www.reddit.com");
        // Configuration binding replaces the default additional addresses
        redditOptions.AdditionalBaseAddresses.ShouldBe(["https://custom.reddit.com", "https://alt.reddit.instance.com"]);
    }

    [Fact]
    public void RedditOptions_EmptyConfiguration_ShouldUseDefault()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddOptions<RedditOptions>()
            .Bind(configuration.GetSection("Reddit"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var redditOptions = serviceProvider.GetRequiredService<IOptions<RedditOptions>>().Value;

        // Assert
        redditOptions.DefaultBaseAddress.ShouldBe("https://www.reddit.com");
        redditOptions.AdditionalBaseAddresses.ShouldBeEmpty();
        // But AllDomains should still include the default reddit.com
        redditOptions.AllDomains.ShouldBe(["www.reddit.com", "reddit.com"]);
    }
}