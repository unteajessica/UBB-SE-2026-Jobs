using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.SkillTests;

public interface ISkillTestRepository
{
    Task<SkillTest?> GetByIdAsync(int skillTestId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SkillTest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<SkillTest> AddAsync(SkillTest skillTest, CancellationToken cancellationToken = default);

    Task UpdateScoreAsync(int skillTestId, int score, CancellationToken cancellationToken = default);

    Task UpdateAchievedDateAsync(int skillTestId, DateOnly achievedDate, CancellationToken cancellationToken = default);

    Task RemoveAsync(int skillTestId, CancellationToken cancellationToken = default);
}
