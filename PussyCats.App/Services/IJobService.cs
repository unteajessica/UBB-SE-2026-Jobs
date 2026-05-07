using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IJobService
{
    Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default);

    Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default);

    Task UpdateAsync(Job job, CancellationToken cancellationToken = default);

    Task RemoveAsync(int jobId, CancellationToken cancellationToken = default);
}
