using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IJobSkillService
{
    Task<JobSkill?> GetByIdAsync(int jobId, int skillId, CancellationToken ct = default);

    Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken ct = default);

    Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken ct = default);

    Task UpdateAsync(JobSkill jobSkill, CancellationToken ct = default);

    Task RemoveAsync(int jobId, int skillId, CancellationToken ct = default);
}
