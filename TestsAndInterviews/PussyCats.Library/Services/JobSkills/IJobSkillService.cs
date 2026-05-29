using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.JobSkills;

public interface IJobSkillService
{
    Task<JobSkill?> GetByIdAsync(int jobId, int skillId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default);

    Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken cancellationToken = default);

    Task UpdateAsync(JobSkill jobSkill, CancellationToken cancellationToken = default);

    Task RemoveAsync(int jobId, int skillId, CancellationToken cancellationToken = default);
}
