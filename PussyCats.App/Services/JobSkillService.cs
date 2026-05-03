using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.App.Services;

public class JobSkillService : IJobSkillService
{
    private readonly IJobSkillRepository jobSkillRepository;

    public JobSkillService(IJobSkillRepository jobSkillRepository)
    {
        this.jobSkillRepository = jobSkillRepository;
    }

    public async Task<JobSkill?> GetByIdAsync(int jobId, int skillId, CancellationToken ct = default)
    {
        return await jobSkillRepository.GetAsync(jobId, skillId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken ct = default)
    {
        return await jobSkillRepository.GetAllAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken ct = default)
    {
        return await jobSkillRepository.GetByJobIdAsync(jobId, ct).ConfigureAwait(false);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken ct = default)
    {
        return await jobSkillRepository.AddAsync(jobSkill, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken ct = default)
    {
        await jobSkillRepository.UpdateAsync(jobSkill, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken ct = default)
    {
        await jobSkillRepository.RemoveAsync(jobId, skillId, ct).ConfigureAwait(false);
    }
}
