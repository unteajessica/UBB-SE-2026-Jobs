using PussyCats.Library.Domain;

namespace PussyCats_App.Services.CompletenessService;

public interface ICompletenessService
{
    int CalculateCompleteness(User? user);
    string GetNextEmptyFieldPrompt(User? user);
}
