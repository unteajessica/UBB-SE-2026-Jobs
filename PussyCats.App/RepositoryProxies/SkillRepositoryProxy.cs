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

    public async Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Skill>(http, $"api/skills/{skillId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Skill>(http, "api/skills", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/skills", skill, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Skill>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/skills/{skill.SkillId}", skill, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int skillId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/skills/{skillId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
