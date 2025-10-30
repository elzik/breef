namespace Elzik.Breef.Domain;

public interface IContentSummarisationInstructionProvider
{
    string GetInstructions(string extractTypeName);
}
