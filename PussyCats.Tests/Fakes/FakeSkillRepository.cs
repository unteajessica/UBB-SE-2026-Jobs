using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Tests.Fakes;

public class FakeSkillRepository : ISkillRepository
{
    private readonly Dictionary<int, Skill> store = new();

    public void Seed(params Skill[] skills)
    {
        foreach (var skill in skills)
        {
            store[skill.SkillId] = skill;
        }
    }

    public Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue(skillId, out var skill);
        return Task.FromResult(skill);
    }

    public Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Skill> snapshot = store.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        if (skill.SkillId == 0)
        {
            skill.SkillId = NextId();
        }
        store[skill.SkillId] = skill;
        return Task.FromResult(skill);
    }

    public Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        store[skill.SkillId] = skill;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int skillId, CancellationToken cancellationToken = default)
    {
        store.Remove(skillId);
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
