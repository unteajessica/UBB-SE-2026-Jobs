using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Skills;

namespace PussyCats.App.ServiceProxies;

public class SkillServiceProxy : ISkillService
{
    private readonly HttpClient http;

    public SkillServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Skill>>("api/skills", cancellationToken) ?? new List<Skill>();

    public async Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/skills/{skillId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Skill>(cancellationToken: cancellationToken);
    }

    public async Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/skills", skill, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Skill>(cancellationToken: cancellationToken))!;
    }

    public async Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/skills/{skill.SkillId}", skill, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int skillId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/skills/{skillId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                ? "Skill cannot be deleted because it is in use."
                : message);
        }
        response.EnsureSuccessStatusCode();
    }
}
