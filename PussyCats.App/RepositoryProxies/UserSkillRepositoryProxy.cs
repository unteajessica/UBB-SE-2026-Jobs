using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.App.RepositoryProxies;

public class UserSkillRepositoryProxy : IUserSkillRepository
{
    private readonly HttpClient http;

    public UserSkillRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<UserSkill>(http, $"api/users/{userId}/skills/{skillId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<UserSkill>(http, $"api/users/{userId}/skills", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<UserSkill>(http, $"api/users/{userId}/skills/verified", ct).ConfigureAwait(false);
    }

    public async Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken ct = default)
    {
        var requestBody = CreateRequestBody(userSkill);
        using var response = await http.PostAsJsonAsync(
            $"api/users/{userSkill.UserId}/skills",
            requestBody,
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<UserSkill>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(UserSkill userSkill, CancellationToken ct = default)
    {
        var requestBody = CreateRequestBody(userSkill);
        using var response = await http.PutAsJsonAsync(
            $"api/users/{userSkill.UserId}/skills/{userSkill.SkillId}",
            requestBody,
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken ct = default)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/users/{userId}/skills/{skillId}/score",
            new { score },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, int skillId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/users/{userId}/skills/{skillId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    private static UserSkill CreateRequestBody(UserSkill userSkill)
    {
        return new UserSkill
        {
            UserId = userSkill.UserId,
            SkillId = userSkill.SkillId,
            Score = userSkill.Score,
            IsVerified = userSkill.IsVerified,
            AchievedDate = userSkill.AchievedDate,
        };
    }
}
