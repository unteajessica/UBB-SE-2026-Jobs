using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats_App.Services.CompatibilityService;

/// <summary>Computes role compatibility scores and skill gap suggestions for a candidate.</summary>
public interface ICompatibilityService
{
    Task<RoleResult> CalculateForRoleAsync(int userId, JobRole role, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleResult>> CalculateAllAsync(int userId, CancellationToken cancellationToken = default);

    IReadOnlyList<Suggestion> GetSuggestions(RoleResult result);
}
