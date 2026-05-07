using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Skills;

public interface IUserSkillRepository
{
    Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default);

    Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken cancellationToken = default);

    Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default);
}
