using Elzik.Breef.Infrastructure.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Elzik.Breef.Api.Tests.Integration;

public class FileBasedContentSummarisationInstructionProviderTests
{
    [Fact]
    public void Instantiated_AllRequiredFilesPresent_Succeeds()
    {
        // Arrange
        var dir = Path.Combine(Path.GetTempPath(), "SummarisationInstructionsIntegrationTest");

        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        File.WriteAllText(Path.Combine(dir, "HtmlContent.md"), "dummy");
        File.WriteAllText(Path.Combine(dir, "RedditPostContent.md"), "dummy");
        File.WriteAllText(Path.Combine(dir, "SubredditContent.md"), "dummy");

        // Act & Assert
        Should.NotThrow(() =>
            new FileBasedContentSummarisationInstructionProvider(
                new NullLogger<FileBasedContentSummarisationInstructionProvider>(),
                dir, ["HtmlContent", "RedditPostContent", "SubredditContent"]));
    }

    [Fact]
    public void Instantiated_SomeRequiredFilesMissing_Throws()
    {
        // Arrange
        var dir = Path.Combine(Path.GetTempPath(), "SummarisationInstructionsIntegrationMissingTest");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        File.WriteAllText(Path.Combine(dir, "HtmlContent.md"), "dummy");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            new FileBasedContentSummarisationInstructionProvider(
                new NullLogger<FileBasedContentSummarisationInstructionProvider>(),
                dir, ["HtmlContent", "RedditPostContent", "SubredditContent"]));
    }

    [Fact]
    public void Instantiated_RequiredInstructionMissing_Throws()
    {
        var dir = Path.Combine(Path.GetTempPath(), "SummarisationInstructionsStartTest");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        Should.Throw<InvalidOperationException>(() =>
        {
            return new FileBasedContentSummarisationInstructionProvider(
                new NullLogger<FileBasedContentSummarisationInstructionProvider>(),
                dir, ["TestMissingExtractor"]);
        });
    }

    [Fact]
    public void Instantiated_DirectoryMissing_Throws()
    {
        var dir = Path.Combine(Path.GetTempPath(), "NonExistentInstructionsDir");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);

        Should.Throw<DirectoryNotFoundException>(() =>
        {
            return new FileBasedContentSummarisationInstructionProvider(
                new NullLogger<FileBasedContentSummarisationInstructionProvider>(),
                dir, ["TestMissingExtractor"]);
        });
    }

    [Theory]
    [InlineData(null)]
    [MemberData(nameof(EmptyArrayTestData))]
    public void Instantiated_InvalidRequiredExtractTypeNames_Throws(string[]? requiredExtractTypeNames)
    {
        // Arrange
        var dir = Path.Combine(Path.GetTempPath(), $"SummarisationInstructions_{Guid.NewGuid()}");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            return new FileBasedContentSummarisationInstructionProvider(
                            new NullLogger<FileBasedContentSummarisationInstructionProvider>(),
                            dir,
                            requiredExtractTypeNames!);
        });

        // Assert
        exception.ParamName.ShouldBe("requiredExtractTypeNames");
        exception.Message.ShouldContain("At least one required extract instruction must be specified.");
    }

    public static IEnumerable<object[]> EmptyArrayTestData()
    {
        yield return new object[] { Array.Empty<string>() };
    }

    [Fact]
    public void GetInstructions_ExtractTypeNameNotFound_Throws()
    {
        // Arrange
        var dir = Path.Combine(Path.GetTempPath(), "SummarisationInstructionsGetTest");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        File.WriteAllText(Path.Combine(dir, "HtmlContent.md"), "dummy content");

        var provider = new FileBasedContentSummarisationInstructionProvider(
            new NullLogger<FileBasedContentSummarisationInstructionProvider>(),
            dir,
            ["HtmlContent"]);

        // Act
        var exception = Should.Throw<InvalidOperationException>(() =>
            provider.GetInstructions("NonExistentType"));

        // Assert
        exception.Message.ShouldContain("No summarisation instructions found for content type 'NonExistentType'.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   \n\t  ")]
    public void Instantiated_EmptyInstructionFile_Throws(string instructionContent)
    {
        // Arrange
        var dir = Path.Combine(Path.GetTempPath(), $"SummarisationInstructions_{Guid.NewGuid()}");
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        File.WriteAllText(Path.Combine(dir, "TestContent.md"), instructionContent);

        // Act
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            return new FileBasedContentSummarisationInstructionProvider(
                        new NullLogger<FileBasedContentSummarisationInstructionProvider>(),
                        dir,
                        ["TestContent"]);
        });

        // Assert
        exception.Message.ShouldContain("Summarisation instruction file is empty:");
        exception.Message.ShouldContain("TestContent.md");
    }
}