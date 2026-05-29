using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;

namespace PussyCats.Tests.Fakes;

public class FakeJobRepository : IJobRepository
{
    private readonly Dictionary<int, Job> jobsById = new();

    public void Seed(params Job[] jobs)
    {
        foreach (var job in jobs)
        {
            jobsById[job.JobId] = job;
        }
    }

    public Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        jobsById.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> snapshot = jobsById.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> filtered = jobsById.Values.Where(job => job.Company.CompanyId == companyId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        if (job.JobId == 0)
        {
            job.JobId = NextId();
        }
        jobsById[job.JobId] = job;
        return Task.FromResult(job);
    }

    public Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        jobsById[job.JobId] = job;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int jobId, CancellationToken cancellationToken = default)
    {
        jobsById.Remove(jobId);
        return Task.CompletedTask;
    }

    private int NextId() => jobsById.Count == 0 ? 1 : jobsById.Keys.Max() + 1;
}
