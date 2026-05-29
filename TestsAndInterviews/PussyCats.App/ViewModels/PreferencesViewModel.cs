using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Preferences;

namespace PussyCats.App.ViewModels;

public class PreferencesViewModel : DispatchableObservableObject
{
    private const int MaximumJobRolesAllowed = 3;

    private readonly IPreferenceService preferencesService;
    private readonly SessionContext session;

    private WorkMode selectedWorkMode;
    private string preferredLocation = string.Empty;
    private List<string> locationSuggestions = new();
    private string errorMessage = string.Empty;

    public PreferencesViewModel(IPreferenceService preferenceService, SessionContext session)
    {
        preferencesService = preferenceService;
        this.session = session;
    }

    public List<JobRole> SelectedJobRoles { get; } = new();

    public WorkMode SelectedWorkMode
    {
        get => selectedWorkMode;
        set => SetProperty(ref selectedWorkMode, value);
    }

    public string PreferredLocation
    {
        get => preferredLocation;
        set => SetProperty(ref preferredLocation, value);
    }

    public List<string> LocationSuggestions
    {
        get => locationSuggestions;
        private set => SetProperty(ref locationSuggestions, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public async Task LoadPreferencesAsync(CancellationToken cancellationToken = default)
    {
        SelectedJobRoles.Clear();
        ErrorMessage = string.Empty;

        var preferences = await preferencesService
            .GetByUserIdAsync(ViewModelSupport.ResolveUserId(session), cancellationToken)
            ;

        foreach (var jobRole in preferences.Roles)
        {
            SelectedJobRoles.Add(jobRole);
        }

        SelectedWorkMode = preferences.WorkMode;
        PreferredLocation = preferences.Location;
        OnPropertyChanged(nameof(SelectedJobRoles));
    }

    public void ToggleJobRole(JobRole jobRole)
    {
        ErrorMessage = string.Empty;

        if (SelectedJobRoles.Contains(jobRole))
        {
            SelectedJobRoles.Remove(jobRole);
        }
        else if (SelectedJobRoles.Count < MaximumJobRolesAllowed)
        {
            SelectedJobRoles.Add(jobRole);
        }
        else
        {
            ErrorMessage = $"You can select a maximum of {MaximumJobRolesAllowed} job roles.";
        }

        OnPropertyChanged(nameof(SelectedJobRoles));
    }

    public void SetWorkMode(WorkMode workMode) => SelectedWorkMode = workMode;

    public void SetLocation(string location) => PreferredLocation = location;

    public async Task SearchLocationAsync(string searchLocationQuery, CancellationToken cancellationToken = default)
    {
        LocationSuggestions = (await preferencesService.SearchLocationsAsync(searchLocationQuery, cancellationToken)).ToList();
    }

    public async Task SavePreferencesAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;
        try
        {
            await preferencesService
                .SavePreferencesAsync(ViewModelSupport.ResolveUserId(session), SelectedJobRoles, SelectedWorkMode, PreferredLocation, cancellationToken)
                ;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public List<JobRole> GetSelectedJobRoles() => SelectedJobRoles;
    public WorkMode GetSelectedWorkMode() => SelectedWorkMode;
    public string GetPreferredLocation() => PreferredLocation;
    public List<string> GetLocationSuggestions() => LocationSuggestions;
    public string GetErrorMessage() => ErrorMessage;
}
