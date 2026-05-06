using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.Library.DTOs;

namespace PussyCats.App.ViewModels;

public class CompatibilityDetailViewModel : ObservableObject
{
    private RoleResult? currentRoleResult;
    private string errorMessage = string.Empty;

    public RoleResult? CurrentRoleResult
    {
        get => currentRoleResult;
        private set => SetProperty(ref currentRoleResult, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public void LoadResult(RoleResult result)
    {
        CurrentRoleResult = result;
        ErrorMessage = string.Empty;
    }

    public double GetMatchScore() => CurrentRoleResult?.MatchScore ?? 0;

    public string GetRoleName()
    {
        return CurrentRoleResult is null
            ? string.Empty
            : ViewModelSupport.FormatJobRole(CurrentRoleResult.JobRole);
    }

    public List<Suggestion> GetSuggestions() => CurrentRoleResult?.Suggestions ?? new List<Suggestion>();
    public string GetErrorMessage() => ErrorMessage;
}
