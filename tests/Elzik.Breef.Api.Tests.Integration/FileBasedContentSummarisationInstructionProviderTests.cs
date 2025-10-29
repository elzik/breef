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
    public void Instantiated_SomeRequiredFilesMissing_Fails()
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
    public void Constructor_Throws_When_RequiredInstructionMissing()
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
    public void Constructor_Throws_When_DirectoryMissing()
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
}