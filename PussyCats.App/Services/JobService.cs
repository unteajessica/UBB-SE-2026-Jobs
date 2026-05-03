using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;

namespace PussyCats.App.Services;

public class JobService : IJobService
{
    private readonly IJobRepository jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        this.jobRepository = jobRepository;
    }

    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken ct = default)
    {
        return await jobRepository.GetByIdAsync(jobId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default)
    {
        return await jobRepository.GetAllAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default)
    {
        return await jobRepository.GetByCompanyIdAsync(companyId, ct).ConfigureAwait(false);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken ct = default)
    {
        return await jobRepository.AddAsync(job, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        await jobRepository.UpdateAsync(job, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, CancellationToken ct = default)
    {
        await jobRepository.RemoveAsync(jobId, ct).ConfigureAwait(false);
    }
}
