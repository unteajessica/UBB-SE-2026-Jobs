using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;

namespace PussyCats.App.RepositoryProxies;

public class SkillTestRepositoryProxy : ISkillTestRepository
{
    private readonly HttpClient http;

    public SkillTestRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<SkillTest?> GetByIdAsync(int skillTestId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<SkillTest>(http, $"api/skill-tests/{skillTestId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SkillTest>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<SkillTest>(http, $"api/skill-tests?userId={userId}", ct).ConfigureAwait(false);
    }

    public async Task<SkillTest> AddAsync(SkillTest skillTest, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/skill-tests", skillTest, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<SkillTest>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateScoreAsync(int skillTestId, int score, CancellationToken ct = default)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/skill-tests/{skillTestId}/score",
            new { score },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateAchievedDateAsync(int skillTestId, DateOnly achievedDate, CancellationToken ct = default)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/skill-tests/{skillTestId}/date",
            new { achievedDate },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int skillTestId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/skill-tests/{skillTestId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
