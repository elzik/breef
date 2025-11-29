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
        result.OriginalUrl.ShouldBe(url);
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

    [Fact]
    public async Task ExtractAsync_WhenCreateUntypedExtractAsyncReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var extractor = new NullReturningExtractor();
        var url = "https://example.com/test";

        // Act
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await extractor.ExtractAsync(url));

        // Assert
        exception.Message.ShouldContain("CreateUntypedExtractAsync returned null");
        exception.Message.ShouldContain(url);
        exception.Message.ShouldContain("NullReturning");
        exception.Message.ShouldContain("A valid UntypedExtract must be returned");
    }

    private class ValidTestExtractor : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new 
                UntypedExtract("Test Title", "Test Content", webPageUrl, "https://example.com/image.jpg"));
        }
    }

    private class AnotherValidExtractor : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new UntypedExtract("Another Title", "Another Content", "https://original.url.com", null));
        }
    }

    private class HtmlContentLikeExtractor : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new UntypedExtract("HTML Title", "HTML Content", "https://original.url.com", null));
        }
    }

    private class InvalidTestClass : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult(new UntypedExtract("Invalid", "Invalid", "https://original.url.com", null));
        }
    }

    private class NullReturningExtractor : ContentExtractorBase
    {
        public override bool CanHandle(string webPageUrl) => true;

        protected override Task<UntypedExtract> CreateUntypedExtractAsync(string webPageUrl)
        {
            return Task.FromResult<UntypedExtract>(null!);
        }
    }
}
