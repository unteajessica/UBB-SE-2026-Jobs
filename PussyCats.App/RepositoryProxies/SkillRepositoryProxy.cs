using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.App.RepositoryProxies;

public class SkillRepositoryProxy : ISkillRepository
{
    private readonly HttpClient http;

    public SkillRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Skill?> GetByIdAsync(int skillId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Skill>(http, $"api/skills/{skillId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Skill>(http, "api/skills", ct).ConfigureAwait(false);
    }

    public async Task<Skill> AddAsync(Skill skill, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/skills", skill, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Skill>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Skill skill, CancellationToken ct = default)
    {
        using var response = await http.PutAsJsonAsync($"api/skills/{skill.SkillId}", skill, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int skillId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/skills/{skillId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
