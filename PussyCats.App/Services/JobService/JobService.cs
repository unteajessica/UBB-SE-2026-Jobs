using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;

namespace PussyCats_App.Services.JobService;

public class JobService : IJobService
{
    private readonly IJobRepository jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        this.jobRepository = jobRepository;
    }

    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await jobRepository.GetByIdAsync(jobId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await jobRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await jobRepository.GetByCompanyIdAsync(companyId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        return await jobRepository.AddAsync(job, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        await jobRepository.UpdateAsync(job, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, CancellationToken cancellationToken = default)
    {
        await jobRepository.RemoveAsync(jobId, cancellationToken).ConfigureAwait(false);
    }
}
