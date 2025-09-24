using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors;

public class ContentExtractorStrategyTests
{
    private readonly Extract _extractedByExtractor1 = new("Title1", "Content1", "Image1");
    private readonly Extract _extractedByExtractor2 = new("Title2", "Content2", "Image2");
    private readonly Extract _extractedByDefaultExtractor = new("DefaultTitle", "DefaultContent", "DefaultImage");

    private readonly IContentExtractor _extractor1 = Substitute.For<IContentExtractor>();
    private readonly IContentExtractor _extractor2 = Substitute.For<IContentExtractor>();
    private readonly IContentExtractor _defaultExtractor = Substitute.For<IContentExtractor>();

    private readonly ContentExtractorStrategy _contentExtractorStrategy;

    private readonly FakeLogger<ContentExtractorStrategy> _fakeLogger;


    public ContentExtractorStrategyTests()
    {
        _extractor1.ExtractAsync(Arg.Any<string>())
            .Returns(ci => { return Task.FromResult(_extractedByExtractor1); });
        _extractor2.ExtractAsync(Arg.Any<string>())
            .Returns(ci => { return Task.FromResult(_extractedByExtractor2); });
        _defaultExtractor.ExtractAsync(Arg.Any<string>())
            .Returns(ci => { return Task.FromResult(_extractedByDefaultExtractor); });
        _defaultExtractor.CanHandle(Arg.Any<string>()).Returns(true);

        _fakeLogger = new FakeLogger<ContentExtractorStrategy>();

        _contentExtractorStrategy = new ContentExtractorStrategy(_fakeLogger, [_extractor1, _extractor2], _defaultExtractor);
    }

    [Fact]
    public async Task ExtractAsync_Extractor1CanHandle_UsesExtractor1()
    {
        // Arrange
        _extractor1.CanHandle(Arg.Any<string>()).Returns(true);
        _extractor2.CanHandle(Arg.Any<string>()).Returns(false);
        
        // Act
        var extract = await _contentExtractorStrategy.ExtractAsync("http://test");

        // Assert
        extract.ShouldBe(_extractedByExtractor1);
        _fakeLogger.Collector.Count.ShouldBe(1);
        _fakeLogger.Collector.LatestRecord.Level.ShouldBe(Microsoft.Extensions.Logging.LogLevel.Information);
        _fakeLogger.Collector.LatestRecord.Message.ShouldStartWith(
            $"Extraction will be provided for by {_extractor1.GetType().Name}");
    }

    [Fact]
    public async Task ExtractAsync_Extractor2CanHandle_UsesExtractor2()
    {
        // Arrange
        _extractor1.CanHandle(Arg.Any<string>()).Returns(false);
        _extractor2.CanHandle(Arg.Any<string>()).Returns(true);

        // Act
        var extract = await _contentExtractorStrategy.ExtractAsync("http://test");

        // Assert
        extract.ShouldBe(_extractedByExtractor2);
        _fakeLogger.Collector.Count.ShouldBe(1);
        _fakeLogger.Collector.LatestRecord.Level.ShouldBe(Microsoft.Extensions.Logging.LogLevel.Information);
        _fakeLogger.Collector.LatestRecord.Message.ShouldStartWith(
            $"Extraction will be provided for by {_extractor1.GetType().Name}");
    }

    [Fact]
    public async Task ExtractAsync_NoSpecificExtractorCanHandle_UsesDefaultExtractor()
    {
        // Arrange
        _extractor1.CanHandle(Arg.Any<string>()).Returns(false);
        _extractor2.CanHandle(Arg.Any<string>()).Returns(false);

        // Act
        var extract = await _contentExtractorStrategy.ExtractAsync("http://test");

        // Assert
        extract.ShouldBe(_extractedByDefaultExtractor);
        _fakeLogger.Collector.Count.ShouldBe(1);
        _fakeLogger.Collector.LatestRecord.Level.ShouldBe(Microsoft.Extensions.Logging.LogLevel.Information);
        _fakeLogger.Collector.LatestRecord.Message.ShouldStartWith(
            $"Extraction will be provided for by {_extractor1.GetType().Name}");
    }

    [Fact]
    public async Task ExtractAsync_OnlyDefaultExtractorExists_UsesDefaultExtractor()
    {
        // Arrange
        _extractor1.CanHandle(Arg.Any<string>()).Returns(true);
        _extractor1.ExtractAsync(Arg.Any<string>())
           .ThrowsAsync(new InvalidOperationException("This extractor (1) should not be used."));
        _extractor2.CanHandle(Arg.Any<string>()).Returns(true);
        _extractor2.ExtractAsync(Arg.Any<string>())
           .ThrowsAsync(new InvalidOperationException("This extractor (2) should not be used."));

        // Act
        var defaultOnlyContentExtractorStrategy = new ContentExtractorStrategy(_fakeLogger, [], _defaultExtractor);
        var extract = await defaultOnlyContentExtractorStrategy.ExtractAsync("http://test");

        // Assert
        extract.ShouldBe(_extractedByDefaultExtractor);
        _fakeLogger.Collector.Count.ShouldBe(1);
        _fakeLogger.Collector.LatestRecord.Level.ShouldBe(Microsoft.Extensions.Logging.LogLevel.Information);
        _fakeLogger.Collector.LatestRecord.Message.ShouldStartWith(
            $"Extraction will be provided for by {_extractor1.GetType().Name}");
    }

    [Fact]
    public void CanHandle_AnyString_CanHandle()
    {
        // Act
        var defaultOnlyContentExtractorStrategy = new ContentExtractorStrategy(_fakeLogger, [], _defaultExtractor);
        var canHandleAnyString = defaultOnlyContentExtractorStrategy.CanHandle("Any string.");

        // Assert
        canHandleAnyString.ShouldBeTrue();
    }

    [Fact]
    public void Instantiated_DefaultExtractorInSpecificExtractors_Throws()
    {
        // Arrange
        var extractor = Substitute.For<IContentExtractor>();

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            new ContentExtractorStrategy(_fakeLogger, [extractor], extractor));

        // Assert
        ex.Message.ShouldBe("Default extractor should not be in the specific extractors list.");
    }

    [Fact]
    public void Instantiated_NullDefaultExtractor_Throws()
    {
        // Arrange
        var extractor = Substitute.For<IContentExtractor>();

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ContentExtractorStrategy(_fakeLogger, [extractor], null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        ex.Message.ShouldBe("Value cannot be null. (Parameter 'defaultExtractor')");
    }

    [Fact]
    public void Instantiated_NullSpecificExtractors_Throws()
    {
        // Arrange
        var defaultExtractor = Substitute.For<IContentExtractor>();

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ContentExtractorStrategy(_fakeLogger, null, defaultExtractor));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        ex.Message.ShouldBe("Value cannot be null. (Parameter 'specificExtractors')");
    }
}
