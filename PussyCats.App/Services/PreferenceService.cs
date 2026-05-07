using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.App.Services;

public class PreferenceService : IPreferenceService
{
    private const string PreferenceTypeJobRole = "JobRole";
    private const string PreferenceTypeWorkMode = "WorkMode";
    private const string PreferenceTypeLocation = "Location";
    private const int MinimumPreferredRoles = 1;
    private const int MaximumPreferredRoles = 3;

    private readonly IUserRepository userRepository;

    public PreferenceService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<IReadOnlyList<Preference>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var preferences = new List<Preference>();

        if (user is null)
        {
            return preferences;
        }

        if (!string.IsNullOrWhiteSpace(user.PreferredEmploymentType))
        {
            foreach (var role in user.PreferredEmploymentType.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                preferences.Add(new Preference
                {
                    UserId = userId,
                    PreferenceType = PreferenceTypeJobRole,
                    Value = role.Trim(),
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(user.WorkModePreference))
        {
            preferences.Add(new Preference
            {
                UserId = userId,
                PreferenceType = PreferenceTypeWorkMode,
                Value = user.WorkModePreference,
            });
        }

        if (!string.IsNullOrWhiteSpace(user.LocationPreference))
        {
            preferences.Add(new Preference
            {
                UserId = userId,
                PreferenceType = PreferenceTypeLocation,
                Value = user.LocationPreference,
            });
        }

        return preferences;
    }

    public async Task SavePreferencesAsync(int userId, IReadOnlyList<JobRole> roles, WorkMode workMode, string location, CancellationToken cancellationToken = default)
    {
        if (roles is null || roles.Count < MinimumPreferredRoles || roles.Count > MaximumPreferredRoles)
        {
            throw new ArgumentException("You must select between 1 and 3 job roles.");
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }

        user.PreferredEmploymentType = string.Join(",", roles.Select(role => role.ToString()));
        user.WorkModePreference = workMode.ToString();
        user.LocationPreference = location;

        await userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<string>> SearchLocationsAsync(string locationQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(locationQuery))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var matches = new List<string>();
        foreach (var location in PredefinedLocations.All)
        {
            if (location.Contains(locationQuery, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(location);
            }
        }
        return Task.FromResult<IReadOnlyList<string>>(matches);
    }
}
