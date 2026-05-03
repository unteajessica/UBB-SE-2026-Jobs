using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.App.Services;

public class UserSkillService : IUserSkillService
{
    private readonly IUserSkillRepository userSkillRepository;

    public UserSkillService(IUserSkillRepository userSkillRepository)
    {
        this.userSkillRepository = userSkillRepository;
    }

    public async Task<UserSkill?> GetByIdAsync(int userId, int skillId, CancellationToken ct = default)
    {
        return await userSkillRepository.GetAsync(userId, skillId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await userSkillRepository.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken ct = default)
    {
        return await userSkillRepository.AddAsync(userSkill, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(UserSkill userSkill, CancellationToken ct = default)
    {
        await userSkillRepository.UpdateAsync(userSkill, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, int skillId, CancellationToken ct = default)
    {
        await userSkillRepository.RemoveAsync(userId, skillId, ct).ConfigureAwait(false);
    }
}
