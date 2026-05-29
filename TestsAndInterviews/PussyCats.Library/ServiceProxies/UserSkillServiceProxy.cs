using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.UserSkillService;


namespace PussyCats.Library.ServiceProxies
{
    public class UserSkillServiceProxy : IUserSkillService
    {
        private readonly HttpClient http;
        public UserSkillServiceProxy(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<UserSkill>>($"api/users/{userId}/skills", cancellationToken) ?? [];

        public async Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<UserSkill>>($"api/users/{userId}/skills/verified", cancellationToken) ?? [];

        public async Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken cancellationToken = default)
        {
            var response = await http.GetAsync($"api/users/{userId}/skills/{skillId}", cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserSkill>(cancellationToken: cancellationToken);
        }

        public async Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
        {
            var response = await http.PostAsJsonAsync($"api/users/{userSkill.User.UserId}/skills", userSkill, cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<UserSkill>(cancellationToken: cancellationToken))!;
        }

        public async Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
        {
            var response = await http.PutAsJsonAsync($"api/users/{userSkill.User.UserId}/skills/{userSkill.Skill.SkillId}", userSkill, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken cancellationToken = default)
        {
            var response = await http.PatchAsJsonAsync($"api/users/{userId}/skills/{skillId}/score", new { Score = score }, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default)
        {
            var response = await http.DeleteAsync($"api/users/{userId}/skills/{skillId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
