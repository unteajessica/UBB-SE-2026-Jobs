using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Jobs;

namespace PussyCats.Library.ServiceProxies;

public class JobServiceProxy : IJobService
{
    private readonly HttpClient http;

    public JobServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Job>>("api/jobs", cancellationToken) ?? new List<Job>();

    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/jobs/{jobId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Job>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Job>>($"api/jobs?companyId={companyId}", cancellationToken) ?? new List<Job>();

    public async Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/jobs", job, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Job>(cancellationToken: cancellationToken))!;
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/jobs/{job.JobId}", job, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/jobs/{jobId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
