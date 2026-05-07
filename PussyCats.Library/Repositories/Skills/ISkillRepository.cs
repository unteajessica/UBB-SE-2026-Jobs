using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Skills;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default);

    Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);

    Task RemoveAsync(int skillId, CancellationToken cancellationToken = default);
}
