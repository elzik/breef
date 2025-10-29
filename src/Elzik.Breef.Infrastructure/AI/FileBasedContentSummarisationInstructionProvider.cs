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

        if(requiredExtractTypeNames == null || !requiredExtractTypeNames.Any())
        {
           throw new ArgumentException("At least one required extract instruction must be specified.", nameof(requiredExtractTypeNames));
        }

        var missing = new List<string>();

        foreach (var extractTypeName in requiredExtractTypeNames)
        {
            var filePath = Path.Combine(instructionFileDirectoryPath, $"{extractTypeName}.md");

            if (!File.Exists(filePath))
            {
                missing.Add($"{extractTypeName}.md");
                continue;
            }

            var instructions = File.ReadAllText(filePath);
            _templatesByKey[extractTypeName] = instructions;
            logger.LogInformation("Loaded summarisation template for {Key} from {FilePath}", extractTypeName, filePath);
        }

        if (missing.Count != 0)
        {
            var missingInstructionsMessage = "Missing summarisation instruction files: " + string.Join(", ", missing);
            throw new InvalidOperationException(missingInstructionsMessage);
        }
    }

    public string GetInstructions(string extractTypeName)
    {
        if (_templatesByKey.TryGetValue(extractTypeName, out var instructions))
        {
            if(string.IsNullOrWhiteSpace(instructions))
            {
                throw new InvalidOperationException(
                    $"Summarisation instructions for content type '{extractTypeName}' are empty.");
            }

            return instructions;
        }

        throw new InvalidOperationException(
            $"No summarisation instructions found for content type '{extractTypeName}'.");
    }
}