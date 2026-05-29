using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Library.Services.JobSkills;

public class JobSkillService : IJobSkillService
{
    private readonly IJobSkillRepository jobSkillRepository;

    public JobSkillService(IJobSkillRepository jobSkillRepository)
    {
        this.jobSkillRepository = jobSkillRepository;
    }

    public async Task<JobSkill?> GetByIdAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        return await jobSkillRepository.GetAsync(jobId, skillId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await jobSkillRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await jobSkillRepository.GetByJobIdAsync(jobId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        return await jobSkillRepository.AddAsync(jobSkill, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        await jobSkillRepository.UpdateAsync(jobSkill, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        await jobSkillRepository.RemoveAsync(jobId, skillId, cancellationToken).ConfigureAwait(false);
    }
}
