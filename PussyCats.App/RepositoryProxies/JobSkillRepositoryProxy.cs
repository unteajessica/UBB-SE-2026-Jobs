using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.App.RepositoryProxies;

public class JobSkillRepositoryProxy : IJobSkillRepository
{
    private readonly HttpClient http;

    public JobSkillRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<JobSkill?> GetAsync(int jobId, int skillId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<JobSkill>(http, $"api/jobs/{jobId}/skills/{skillId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<JobSkill>(http, "api/job-skills", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<JobSkill>(http, $"api/jobs/{jobId}/skills", ct).ConfigureAwait(false);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken ct = default)
    {
        var requestBody = CreateRequestBody(jobSkill);
        using var response = await http.PostAsJsonAsync(
            $"api/jobs/{jobSkill.JobId}/skills",
            requestBody,
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<JobSkill>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken ct = default)
    {
        var requestBody = CreateRequestBody(jobSkill);
        using var response = await http.PutAsJsonAsync(
            $"api/jobs/{jobSkill.JobId}/skills/{jobSkill.SkillId}",
            requestBody,
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/jobs/{jobId}/skills/{skillId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    private static JobSkill CreateRequestBody(JobSkill jobSkill)
    {
        return new JobSkill
        {
            JobId = jobSkill.JobId,
            SkillId = jobSkill.SkillId,
            RequiredLevel = jobSkill.RequiredLevel,
        };
    }
}
