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

    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Job>(http, $"api/jobs/{jobId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Job>(http, "api/jobs", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Job>(http, $"api/jobs?companyId={companyId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/jobs", job, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Job>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/jobs/{job.JobId}", job, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/jobs/{jobId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
