using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Tests.Fakes;

public class FakeSkillGroupRepository : ISkillGroupRepository
{
    private readonly Dictionary<int, SkillGroup> skillGroupsById = new();

    public void Seed(params SkillGroup[] skillGroups)
    {
        foreach (var skillGroup in skillGroups)
        {
            skillGroupsById[skillGroup.SkillGroupId] = skillGroup;
        }
    }

    public Task<IReadOnlyList<SkillGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SkillGroup> snapshot = skillGroupsById.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<SkillGroup>> GetByJobRoleAsync(JobRole jobRole, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SkillGroup> filtered = skillGroupsById.Values.Where(skillGroup => skillGroup.JobRole == jobRole).ToList();
        return Task.FromResult(filtered);
    }
}
