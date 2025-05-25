using Elzik.Breef.Domain;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit;

public class ContentExtractorStrategyTests
{
    private readonly Extract extractedByExtractor1 = new("Title1", "Content1", "Image1");
    private readonly Extract extractedByExtractor2 = new("Title2", "Content2", "Image2");
    private readonly Extract extractedByDefaultExtractor = new("DefaultTitle", "DefaultContent", "DefaultImage");

    private readonly IContentExtractor extractor1 = Substitute.For<IContentExtractor>();
    private readonly IContentExtractor extractor2 = Substitute.For<IContentExtractor>();
    private readonly IContentExtractor defaultExtractor = Substitute.For<IContentExtractor>();

    private readonly ContentExtractorStrategy contentExtractorStrategy;


    public ContentExtractorStrategyTests()
    {
        extractor1.ExtractAsync(Arg.Any<string>())
            .Returns(ci => { return Task.FromResult(extractedByExtractor1); });
        extractor2.ExtractAsync(Arg.Any<string>())
            .Returns(ci => { return Task.FromResult(extractedByExtractor2); });
        defaultExtractor.ExtractAsync(Arg.Any<string>())
            .Returns(ci => { return Task.FromResult(extractedByDefaultExtractor); });
        defaultExtractor.CanHandle(Arg.Any<string>()).Returns(true);

        contentExtractorStrategy = new ContentExtractorStrategy([extractor1, extractor2], defaultExtractor);
    }

    [Fact]
    public async Task ExtractAsync_Extractor1CanHandle_UsesExtractor1()
    {
        // Arrange
        extractor1.CanHandle(Arg.Any<string>()).Returns(true);
        extractor2.CanHandle(Arg.Any<string>()).Returns(false);
        
        // Act
        var extract = await contentExtractorStrategy.ExtractAsync("http://test");

        // Assert
        extract.ShouldBe(extractedByExtractor1);
    }

    [Fact]
    public async Task ExtractAsync_Extractor2CanHandle_UsesExtractor2()
    {
        // Arrange
        extractor1.CanHandle(Arg.Any<string>()).Returns(false);
        extractor2.CanHandle(Arg.Any<string>()).Returns(true);

        // Act
        var extract = await contentExtractorStrategy.ExtractAsync("http://test");

        // Assert
        extract.ShouldBe(extractedByExtractor2);
    }

    [Fact]
    public async Task ExtractAsync_NoSpecificExtractorCanHandle_UsesDefaultExtractor()
    {
        // Arrange
        extractor1.CanHandle(Arg.Any<string>()).Returns(false);
        extractor2.CanHandle(Arg.Any<string>()).Returns(false);

        // Act
        var extract = await contentExtractorStrategy.ExtractAsync("http://test");

        // Assert
        extract.ShouldBe(extractedByDefaultExtractor);
    }

    [Fact]
    public void Throws_If_DefaultExtractor_In_SpecificExtractors()
    {
        // Arrange
        var extractor = Substitute.For<IContentExtractor>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            new ContentExtractorStrategy([extractor], extractor));

        // Assert
        ex.Message.ShouldBe("Default extractor should not be in the specific extractors list.");
    }

    [Fact]
    public void CanHandle_Always_Returns_True()
    {
        // Arrange
        var extractor = Substitute.For<IContentExtractor>();
        var defaultExtractor = Substitute.For<IContentExtractor>();
        var strategy = new ContentExtractorStrategy([extractor], defaultExtractor);

        // Act & Assert
        Assert.True(strategy.CanHandle("http://any-url"));
    }
}
