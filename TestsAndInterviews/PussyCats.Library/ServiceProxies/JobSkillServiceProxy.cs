using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.JobSkills;

namespace PussyCats.Library.ServiceProxies;

public class JobSkillServiceProxy : IJobSkillService
{
    private readonly HttpClient http;

    public JobSkillServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<JobSkill>> GetAllAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<JobSkill>>("api/job-skills", JsonOptions.Default, cancellationToken) ?? new List<JobSkill>();

    public async Task<IReadOnlyList<JobSkill>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<JobSkill>>($"api/jobs/{jobId}/skills", JsonOptions.Default, cancellationToken) ?? new List<JobSkill>();

    public async Task<JobSkill?> GetByIdAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/jobs/{jobId}/skills/{skillId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JobSkill>(JsonOptions.Default, cancellationToken);
    }

    public async Task<JobSkill> AddAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync($"api/jobs/{jobSkill.Job.JobId}/skills", jobSkill, JsonOptions.Default, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobSkill>(JsonOptions.Default, cancellationToken))!;
    }

    public async Task UpdateAsync(JobSkill jobSkill, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/jobs/{jobSkill.Job.JobId}/skills/{jobSkill.Skill.SkillId}", jobSkill, JsonOptions.Default, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int jobId, int skillId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/jobs/{jobId}/skills/{skillId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
