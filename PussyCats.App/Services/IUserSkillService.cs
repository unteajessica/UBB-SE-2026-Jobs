using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IUserSkillService
{
    Task<UserSkill?> GetByIdAsync(int userId, int skillId, CancellationToken ct = default);

    Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken ct = default);

    Task UpdateAsync(UserSkill userSkill, CancellationToken ct = default);

    Task RemoveAsync(int userId, int skillId, CancellationToken ct = default);
}
