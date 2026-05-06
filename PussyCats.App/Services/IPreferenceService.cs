using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

/// <summary>Reads and writes the three user preference fields: employment type, work mode, location.</summary>
public interface IPreferenceService
{
    Task<IReadOnlyList<Preference>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task SavePreferencesAsync(int userId, IReadOnlyList<JobRole> roles, WorkMode workMode, string location, CancellationToken ct = default);

    Task<IReadOnlyList<string>> SearchLocationsAsync(string locationQuery, CancellationToken ct = default);
}
