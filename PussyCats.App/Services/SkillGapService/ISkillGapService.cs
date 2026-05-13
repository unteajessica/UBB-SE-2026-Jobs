using PussyCats.Library.DTOs;

namespace PussyCats_App.Services.SkillGapService;

public interface ISkillGapService
{
    Task<IReadOnlyList<MissingSkillModel>> GetMissingSkillsAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnderscoredSkillModel>> GetUnderscoredSkillsAsync(int userId, CancellationToken cancellationToken = default);

    Task<SkillGapSummaryModel> GetSummaryAsync(int userId, CancellationToken cancellationToken = default);
}
