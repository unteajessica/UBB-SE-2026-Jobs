using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;

namespace PussyCats.Tests.Fakes;

public class FakeJobRepository : IJobRepository
{
    private readonly Dictionary<int, Job> store = new();

    public void Seed(params Job[] jobs)
    {
        foreach (var job in jobs)
        {
            store[job.JobId] = job;
        }
    }

    public Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> snapshot = store.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> filtered = store.Values.Where(job => job.Company.CompanyId == companyId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        if (job.JobId == 0)
        {
            job.JobId = NextId();
        }
        store[job.JobId] = job;
        return Task.FromResult(job);
    }

    public Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        store[job.JobId] = job;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int jobId, CancellationToken cancellationToken = default)
    {
        store.Remove(jobId);
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
