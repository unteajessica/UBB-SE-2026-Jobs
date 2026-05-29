using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.DTOs;

public sealed record UserPreferences(
    IReadOnlyList<JobRole> Roles,
    WorkMode WorkMode,
    string Location);
