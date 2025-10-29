using Elzik.Breef.Domain;
using Elzik.Breef.Infrastructure.ContentExtractors;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Unit.ContentExtractors;

public class ContentExtractorBaseTests
{
    [Fact]
    public void Constructor_ValidExtractorName_DoesNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => new ValidTestExtractor());
    }

    [Fact]
    public void Constructor_InvalidExtractorNameWithoutSuffix_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => new InvalidTestClass());
        exception.Message.ShouldContain("InvalidTestClass");
        exception.Message.ShouldContain("must end with 'Extractor' suffix");
        exception.Message.ShouldContain("derive the ExtractType");
    }

    [Fact]
    public async Task ExtractAsync_WhenCalled_SetsExtractTypeCorrectly()
    {
        // Arrange
        var extractor = new ValidTestExtractor();
        var url = "https://example.com";

        // Act
        var result = await extractor.ExtractAsync(url);

        // Assert
        result.ExtractType.ShouldBe("ValidTest");
    }

    [Fact]
    public async Task ExtractAsync_WhenCalled_PreservesExtractDataFromCore()
    {
        // Arrange
        var extractor = new ValidTestExtractor();
        var url = "https://example.com";

        // Act
        var result = await extractor.ExtractAsync(url);

        // Assert
        result.Title.ShouldBe("Test Title");
        result.Content.ShouldBe("Test Content");
        result.PreviewImageUrl.ShouldBe("https://example.com/image.jpg");
    }

    [Fact]
    public async Task ExtractAsync_MultipleExtractorTypes_SetsDifferentExtractTypes()
    {
        // Arrange
        var extractor1 = new ValidTestExtractor();
        var extractor2 = new AnotherValidExtractor();

        // Act
        var result1 = await extractor1.ExtractAsync("https://example.com");
        var result2 = await extractor2.ExtractAsync("https://example.com");

        // Assert
        result1.ExtractType.ShouldBe("ValidTest");
        result2.ExtractType.ShouldBe("AnotherValid");
    }

    [Fact]
    public async Task ExtractAsync_ExtractorNameEndingWithExtractor_RemovesSuffixCorrectly()
    {
        // Arrange
        var extractor = new HtmlContentLikeExtractor();

        // Act
        var result = await extractor.ExtractAsync("https://example.com");

        // Assert
        result.ExtractType.ShouldBe("HtmlContentLike");
    }

    private class ValidTestExtractor : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new UntypedExtract("Test Title", "Test Content", "https://example.com/image.jpg"));
        }
    }

    private class AnotherValidExtractor : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new UntypedExtract("Another Title", "Another Content", null));
        }
    }

    private class HtmlContentLikeExtractor : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new UntypedExtract("HTML Title", "HTML Content", null));
        }
    }

    private class InvalidTestClass : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new UntypedExtract("Invalid", "Invalid", null));
        }
    }
}
