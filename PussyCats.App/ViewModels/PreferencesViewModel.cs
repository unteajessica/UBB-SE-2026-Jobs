using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.ViewModels;

public class PreferencesViewModel : ObservableObject
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

    public async Task LoadPreferencesAsync(CancellationToken ct = default)
    {
        SelectedJobRoles.Clear();
        ErrorMessage = string.Empty;

        var savedPreferences = await preferencesService
            .GetByUserIdAsync(ViewModelSupport.ResolveUserId(session), ct)
            .ConfigureAwait(false);

        foreach (var preference in savedPreferences)
        {
            if (preference.PreferenceType == "JobRole" &&
                Enum.TryParse<JobRole>(preference.Value, out var jobRole))
            {
                SelectedJobRoles.Add(jobRole);
            }
            else if (preference.PreferenceType == "WorkMode" &&
                     Enum.TryParse<WorkMode>(preference.Value, out var workMode))
            {
                SelectedWorkMode = workMode;
            }
            else if (preference.PreferenceType == "Location")
            {
                PreferredLocation = preference.Value;
            }
        }

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

    public async Task SearchLocationAsync(string searchLocationQuery, CancellationToken ct = default)
    {
        LocationSuggestions = (await preferencesService.SearchLocationsAsync(searchLocationQuery, ct).ConfigureAwait(false)).ToList();
    }

    public async Task SavePreferencesAsync(CancellationToken ct = default)
    {
        ErrorMessage = string.Empty;
        try
        {
            await preferencesService
                .SavePreferencesAsync(ViewModelSupport.ResolveUserId(session), SelectedJobRoles, SelectedWorkMode, PreferredLocation, ct)
                .ConfigureAwait(false);
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
