using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;

namespace PussyCats.App.RepositoryProxies;

public class JobRepositoryProxy : IJobRepository
{
    private readonly HttpClient http;

    public JobRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Job>(http, $"api/jobs/{jobId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Job>(http, "api/jobs", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Job>(http, $"api/jobs?companyId={companyId}", ct).ConfigureAwait(false);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/jobs", job, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Job>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        using var response = await http.PutAsJsonAsync($"api/jobs/{job.JobId}", job, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/jobs/{jobId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
