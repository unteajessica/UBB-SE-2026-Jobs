using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Library.Services.Skills;

public class SkillService : ISkillService
{
    private readonly ISkillRepository repository;

    public SkillService(ISkillRepository repository)
    {
        this.repository = repository;
    }

    public Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(skillId, cancellationToken);

    public Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
        => repository.AddAsync(skill, cancellationToken);

    public Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(skill, cancellationToken);

    public Task RemoveAsync(int skillId, CancellationToken cancellationToken = default)
        => repository.RemoveAsync(skillId, cancellationToken);
}