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

    public async Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<UserSkill>(http, $"api/users/{userId}/skills/{skillId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<UserSkill>(http, $"api/users/{userId}/skills", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<UserSkill>(http, $"api/users/{userId}/skills/verified", cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        var requestBody = CreateRequestBody(userSkill);
        using var response = await http.PostAsJsonAsync(
            $"api/users/{userSkill.User.UserId}/skills",
            requestBody,
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<UserSkill>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        var requestBody = CreateRequestBody(userSkill);
        using var response = await http.PutAsJsonAsync(
            $"api/users/{userSkill.User.UserId}/skills/{userSkill.Skill.SkillId}",
            requestBody,
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken cancellationToken = default)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/users/{userId}/skills/{skillId}/score",
            new { score },
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/users/{userId}/skills/{skillId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    private static UserSkill CreateRequestBody(UserSkill userSkill)
    {
        return new UserSkill
        {
            User = new User { UserId = userSkill.User.UserId },
            Skill = new Skill { SkillId = userSkill.Skill.SkillId },
            Score = userSkill.Score,
            IsVerified = userSkill.IsVerified,
            AchievedDate = userSkill.AchievedDate,
        };
    }
}
