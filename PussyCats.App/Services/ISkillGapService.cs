using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

public interface ISkillGapService
{
    Task<IReadOnlyList<MissingSkillModel>> GetMissingSkillsAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<UnderscoredSkillModel>> GetUnderscoredSkillsAsync(int userId, CancellationToken ct = default);

    Task<SkillGapSummaryModel> GetSummaryAsync(int userId, CancellationToken ct = default);
}
