using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Users;

namespace PussyCats_App.Services.PreferenceService;

public class PreferenceService : IPreferenceService
{
    private const int MinimumPreferredRoles = 1;
    private const int MaximumPreferredRoles = 3;

    private readonly IUserRepository userRepository;

    public PreferenceService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<UserPreferences> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return new UserPreferences([], default, string.Empty);
        }

        var roles = new List<JobRole>();
        foreach (var role in user.PreferredEmploymentType.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse<JobRole>(role, out var parsedRole))
            {
                roles.Add(parsedRole);
            }
        }

        var workMode = Enum.TryParse<WorkMode>(user.WorkModePreference, out var parsedWorkMode)
            ? parsedWorkMode
            : default;

        return new UserPreferences(roles, workMode, user.LocationPreference);
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
