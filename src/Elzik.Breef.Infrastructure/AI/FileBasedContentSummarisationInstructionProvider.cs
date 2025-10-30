using Elzik.Breef.Domain;
using Microsoft.Extensions.Logging;

namespace Elzik.Breef.Infrastructure.AI;

public sealed class FileBasedContentSummarisationInstructionProvider : IContentSummarisationInstructionProvider
{
    private readonly Dictionary<string, string> _templatesByKey = new(StringComparer.OrdinalIgnoreCase);

    public FileBasedContentSummarisationInstructionProvider(
    ILogger<FileBasedContentSummarisationInstructionProvider> logger,
    string instructionFileDirectoryPath,
    IEnumerable<string> requiredExtractTypeNames)
    {
        if (!Directory.Exists(instructionFileDirectoryPath))
        {
            throw new DirectoryNotFoundException($"Summarisation instructions directory not found at: {instructionFileDirectoryPath}");
        }

        if (requiredExtractTypeNames == null || !requiredExtractTypeNames.Any())
        {
            throw new ArgumentException("At least one required extract instruction must be specified.", nameof(requiredExtractTypeNames));
        }

        foreach (var extractTypeName in requiredExtractTypeNames)
        {
            var filePath = Path.Combine(instructionFileDirectoryPath, $"{extractTypeName}.md");

            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException($"Missing summarisation instruction file: {filePath}");
            }

            var instructions = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(instructions))
            {
                throw new InvalidOperationException($"Summarisation instruction file is empty: {filePath}");
            }

            _templatesByKey[extractTypeName] = instructions;
            logger.LogInformation("Loaded summarisation template for {Key} from {FilePath}", extractTypeName, filePath);
        }
    }

    public string GetInstructions(string extractTypeName)
    {
        if (_templatesByKey.TryGetValue(extractTypeName, out var instructions))
        {
            return instructions;
        }

        throw new InvalidOperationException(
            $"No summarisation instructions found for content type '{extractTypeName}'.");
    }
}