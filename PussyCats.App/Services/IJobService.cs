using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IJobService
{
    Task<Job?> GetByIdAsync(int jobId, CancellationToken ct = default);

    Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default);

    Task<Job> AddAsync(Job job, CancellationToken ct = default);

    Task UpdateAsync(Job job, CancellationToken ct = default);

    Task RemoveAsync(int jobId, CancellationToken ct = default);
}
