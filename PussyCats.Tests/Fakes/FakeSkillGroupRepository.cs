using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Tests.Fakes;

public class FakeSkillGroupRepository : ISkillGroupRepository
{
    private readonly Dictionary<int, SkillGroup> store = new();

    public void Seed(params SkillGroup[] skillGroups)
    {
        foreach (var skillGroup in skillGroups)
        {
            store[skillGroup.SkillGroupId] = skillGroup;
        }
    }

    public Task<IReadOnlyList<SkillGroup>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<SkillGroup> snapshot = store.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<SkillGroup>> GetByJobRoleAsync(JobRole jobRole, CancellationToken ct = default)
    {
        IReadOnlyList<SkillGroup> filtered = store.Values.Where(g => g.JobRole == jobRole).ToList();
        return Task.FromResult(filtered);
    }
}
