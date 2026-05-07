using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IUserSkillService
{
    Task<UserSkill?> GetByIdAsync(int userId, int skillId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default);

    Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default);
}
