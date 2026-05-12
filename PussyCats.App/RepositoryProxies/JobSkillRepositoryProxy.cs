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

    public async Task<JobSkill?> GetAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<JobSkill>(http, $"api/jobs/{jobId}/skills/{skillId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<JobSkill>(http, "api/job-skills", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<JobSkill>(http, $"api/jobs/{jobId}/skills", cancellationToken).ConfigureAwait(false);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        var requestBody = CreateRequestBody(jobSkill);
        using var response = await http.PostAsJsonAsync(
            $"api/jobs/{jobSkill.Job.JobId}/skills",
            requestBody,
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<JobSkill>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        var requestBody = CreateRequestBody(jobSkill);
        using var response = await http.PutAsJsonAsync(
            $"api/jobs/{jobSkill.Job.JobId}/skills/{jobSkill.Skill.SkillId}",
            requestBody,
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/jobs/{jobId}/skills/{skillId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    private static JobSkill CreateRequestBody(JobSkill jobSkill)
    {
        return new JobSkill
        {
            Job = jobSkill.Job,
            Skill = jobSkill.Skill,
            RequiredLevel = jobSkill.RequiredLevel,
        };
    }
}
