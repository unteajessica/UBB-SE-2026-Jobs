using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats_App.Services.UserSkillService;

public class UserSkillService : IUserSkillService
{
    private readonly IUserSkillRepository userSkillRepository;

    public UserSkillService(IUserSkillRepository userSkillRepository)
    {
        this.userSkillRepository = userSkillRepository;
    }

    public async Task<UserSkill?> GetByIdAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        return await userSkillRepository.GetAsync(userId, skillId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await userSkillRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        return await userSkillRepository.AddAsync(userSkill, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        await userSkillRepository.UpdateAsync(userSkill, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        await userSkillRepository.RemoveAsync(userId, skillId, cancellationToken).ConfigureAwait(false);
    }
}
