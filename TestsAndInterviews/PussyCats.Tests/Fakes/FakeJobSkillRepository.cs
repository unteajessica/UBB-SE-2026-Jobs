using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Tests.Fakes;

public class FakeJobSkillRepository : IJobSkillRepository
{
    private readonly Dictionary<(int JobId, int SkillId), JobSkill> jobSkillsById = new();

    public void Seed(params JobSkill[] jobSkills)
    {
        foreach (var jobSkill in jobSkills)
        {
            jobSkillsById[(jobSkill.Job.JobId, jobSkill.Skill.SkillId)] = jobSkill;
        }
    }

    public Task<JobSkill?> GetAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        jobSkillsById.TryGetValue((jobId, skillId), out var jobSkill);
        return Task.FromResult(jobSkill);
    }

    public Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<JobSkill> snapshot = jobSkillsById.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<JobSkill> filtered = jobSkillsById.Values.Where(jobSkill => jobSkill.Job.JobId == jobId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        jobSkillsById[(jobSkill.Job.JobId, jobSkill.Skill.SkillId)] = jobSkill;
        return Task.FromResult(jobSkill);
    }

    public Task UpdateAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        jobSkillsById[(jobSkill.Job.JobId, jobSkill.Skill.SkillId)] = jobSkill;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        jobSkillsById.Remove((jobId, skillId));
        return Task.CompletedTask;
    }
}
