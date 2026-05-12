using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Tests.Fakes;

public class FakeSkillRepository : ISkillRepository
{
    private readonly Dictionary<int, Skill> skillsById = new();

    public void Seed(params Skill[] skills)
    {
        foreach (var skill in skills)
        {
            skillsById[skill.SkillId] = skill;
        }
    }

    public Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        skillsById.TryGetValue(skillId, out var skill);
        return Task.FromResult(skill);
    }

    public Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Skill> snapshot = skillsById.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        if (skill.SkillId == 0)
        {
            skill.SkillId = NextId();
        }
        skillsById[skill.SkillId] = skill;
        return Task.FromResult(skill);
    }

    public Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        skillsById[skill.SkillId] = skill;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int skillId, CancellationToken cancellationToken = default)
    {
        skillsById.Remove(skillId);
        return Task.CompletedTask;
    }

    private int NextId() => skillsById.Count == 0 ? 1 : skillsById.Keys.Max() + 1;
}
