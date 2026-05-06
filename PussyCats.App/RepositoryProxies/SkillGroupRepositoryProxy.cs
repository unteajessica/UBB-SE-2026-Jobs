using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.App.RepositoryProxies;

public class SkillGroupRepositoryProxy : ISkillGroupRepository
{
    private readonly HttpClient http;

    public SkillGroupRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<SkillGroup>> GetAllAsync(CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<SkillGroup>(http, "api/skill-groups", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SkillGroup>> GetByJobRoleAsync(JobRole jobRole, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<SkillGroup>(http, $"api/skill-groups?jobRole={jobRole}", ct).ConfigureAwait(false);
    }
}
