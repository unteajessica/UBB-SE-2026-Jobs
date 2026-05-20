using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.SkillTests;

public interface ISkillTestService
{
    Task<IReadOnlyList<SkillTest>> GetTestsForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> CanRetakeTestAsync(int skillTestId, CancellationToken cancellationToken = default);

    Task<Badge> SubmitRetakeAsync(int skillTestId, int newScore, CancellationToken cancellationToken = default);

    Task<SkillTest?> GetSkillTestByIdAsync(int skillTestId, CancellationToken cancellationToken = default);

    Task<SkillTest> AddSkillTestAsync(SkillTest skillTest, CancellationToken cancellationToken = default);

    Task UpdateScoreAsync(int skillTestId, int newScore, CancellationToken cancellationToken = default);

    Task UpdateAchievedDateAsync(int skillTestId, DateOnly newDate, CancellationToken cancellationToken = default);
    Task RemoveAsync(int skillTestId, CancellationToken cancellationToken = default);
}
