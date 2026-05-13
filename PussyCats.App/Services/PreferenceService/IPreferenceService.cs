using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats_App.Services.PreferenceService;

/// <summary>Reads and writes the three user preference fields: employment type, work mode, location.</summary>
public interface IPreferenceService
{
    Task<UserPreferences> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task SavePreferencesAsync(int userId, IReadOnlyList<JobRole> roles, WorkMode workMode, string location, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> SearchLocationsAsync(string locationQuery, CancellationToken cancellationToken = default);
}
