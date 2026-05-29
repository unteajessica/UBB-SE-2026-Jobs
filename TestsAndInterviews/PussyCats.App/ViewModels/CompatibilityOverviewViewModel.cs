using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.CompatibilityService;

namespace PussyCats.App.ViewModels;

public class CompatibilityOverviewViewModel : DispatchableObservableObject
{
    private readonly ICompatibilityService compatibilityService;
    private readonly SessionContext session;
    private List<RoleResult> roleResults = new();
    private RoleResult? selectedResult;
    private string errorMessage = string.Empty;

    public CompatibilityOverviewViewModel(ICompatibilityService compatibilityService, SessionContext session)
    {
        this.compatibilityService = compatibilityService;
        this.session = session;
    }

    public List<RoleResult> RoleResults
    {
        get => roleResults;
        private set => SetProperty(ref roleResults, value);
    }

    public RoleResult? SelectedResult
    {
        get => selectedResult;
        private set => SetProperty(ref selectedResult, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public async Task LoadAllRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            RoleResults = (await compatibilityService
                .CalculateAllAsync(ViewModelSupport.ResolveUserId(session), cancellationToken)
                ).ToList();
            ErrorMessage = string.Empty;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public List<RoleResult> GetRoleResults() => RoleResults;

    public RoleResult? GetResultForRole(JobRole role)
    {
        return RoleResults.FirstOrDefault(result => result.JobRole == role);
    }

    public void OnRoleSelected(JobRole role)
    {
        SelectedResult = GetResultForRole(role);
    }

    public RoleResult? GetSelectedResult() => SelectedResult;
    public string GetErrorMessage() => ErrorMessage;
}
