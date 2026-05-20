using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.CompletenessService;

public interface ICompletenessService
{
    int CalculateCompleteness(User? user);
    string GetNextEmptyFieldPrompt(User? user);
}
