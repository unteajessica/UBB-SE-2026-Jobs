using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.SkillTests;


namespace PussyCats.Library.ServiceProxies
{
    public class SkillTestServiceProxy : ISkillTestService
    {
        private readonly HttpClient httpClient;

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public SkillTestServiceProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }


        public async Task<IReadOnlyList<SkillTest>> GetTestsForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await httpClient.GetFromJsonAsync<List<SkillTest>>($"api/skill-tests?userId={userId}", jsonSerializerOptions, cancellationToken) ?? new List<SkillTest>();
        }

        public async Task<SkillTest?> GetSkillTestByIdAsync(int skillTestId, CancellationToken cancellationToken = default)
        {
            return await httpClient.GetFromJsonAsync<SkillTest>($"api/skill-tests/{skillTestId}", jsonSerializerOptions, cancellationToken);
        }

        public async Task<SkillTest> AddSkillTestAsync(SkillTest skillTest, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync("api/skill-tests", skillTest, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SkillTest>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Response content was null");
        }

        public async Task UpdateScoreAsync(int skillTestId, int newScore, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PatchAsync($"api/skill-tests/{skillTestId}/score", JsonContent.Create(new { Score = newScore }), cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAchievedDateAsync(int skillTestId, DateOnly newDate, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PatchAsync($"api/skill-tests/{skillTestId}/date", JsonContent.Create(new { AchievedDate = newDate }), cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveAsync(int skillTestId, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.DeleteAsync($"api/skill-tests/{skillTestId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        // Implement CanRetakeTestAsync and SubmitRetakeAsync similarly using httpClient.GetAsync or PostAsync
        public async Task<bool> CanRetakeTestAsync(int skillTestId, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.GetAsync($"api/skill-tests/{skillTestId}/retake-eligibility", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RetakeEligibilityResponse>(cancellationToken: cancellationToken);
                return result.CanRetake;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"No test found for ID {skillTestId}");
            }
            else
            {
                response.EnsureSuccessStatusCode(); // Throw for other non-success status codes
                return false; // This line will never be reached, but is required to satisfy the compiler
            }
        }
        public async Task<Badge> SubmitRetakeAsync(int skillTestId, int newScore, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync($"api/skill-tests/{skillTestId}/retake", new { Score = newScore }, jsonSerializerOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Badge>(jsonSerializerOptions, cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Response content was null");
        }
    }

    public record RetakeEligibilityResponse(bool CanRetake);
}
