using System.Net.Http.Json;
using PussyCats.App.Dtos.TI;

namespace PussyCats.App.Services.TI;

public interface ITiTestService
{
    Task<List<TiTestDto>> GetAllAsync();
    Task<List<TiTestDto>> GetByCategoryAsync(string category);
    Task<TiTestDto?> GetByIdAsync(int testId);
    Task<List<TiQuestionDto>> GetQuestionsByTestIdAsync(int testId);
    Task StartAttemptAsync(int userId, int testId);
    Task<TiTestAttemptDto?> GetAttemptByUserAndTestAsync(int userId, int testId);
    Task<float> SubmitAttemptAsync(int userId, int testId, IEnumerable<TiAnswerDto> answers);
    Task<bool> AttemptExistsAsync(int userId, int testId);
    Task<List<TiAnswerDto>> GetAnswersByAttemptAsync(int attemptId);
}

public class TiTestService : ITiTestService
{
    private readonly HttpClient http;

    public TiTestService(HttpClient http) => this.http = http;

    public async Task<List<TiTestDto>> GetAllAsync()
    {
        var response = await http.GetAsync("api/tests");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiTestDto>>() ?? new();
    }

    public async Task<List<TiTestDto>> GetByCategoryAsync(string category)
    {
        var response = await http.GetAsync($"api/tests/bycategory/{category}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiTestDto>>() ?? new();
    }

    public async Task<TiTestDto?> GetByIdAsync(int testId)
    {
        var response = await http.GetAsync($"api/tests/{testId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TiTestDto>();
    }

    public async Task<List<TiQuestionDto>> GetQuestionsByTestIdAsync(int testId)
    {
        var response = await http.GetAsync($"api/questions/bytest/{testId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiQuestionDto>>() ?? new();
    }

    public async Task StartAttemptAsync(int userId, int testId)
    {
        var payload = new { UserId = userId, ExternalUserId = userId, TestId = testId, Status = "IN_PROGRESS", StartedAt = DateTime.UtcNow };
        var response = await http.PostAsJsonAsync("api/testattempts", payload);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            throw new InvalidOperationException("Test already attempted.");
        response.EnsureSuccessStatusCode();
    }

    public async Task<TiTestAttemptDto?> GetAttemptByUserAndTestAsync(int userId, int testId)
    {
        var response = await http.GetAsync($"api/testattempts/byuser/{userId}/bytest/{testId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TiTestAttemptDto>();
    }

    public async Task<float> SubmitAttemptAsync(int userId, int testId, IEnumerable<TiAnswerDto> answers)
    {
        // Submit through the TI composite endpoint, which saves the answers, grades them,
        // computes the score, marks the attempt COMPLETED (with score + completed time),
        // persists it via a tracked entity, and recalculates the leaderboard — server-side.
        // (Replaces a client-side reimplementation whose PUT api/testattempts/{id} silently
        // no-opped, leaving every submitted attempt stuck IN_PROGRESS with no score.)
        var payload = new
        {
            UserId = userId,
            TestId = testId,
            Answers = answers.Select(a => new { a.QuestionId, a.Value }).ToList(),
        };

        var response = await http.PostAsJsonAsync("api/tests/submit-attempt", payload);
        if (!response.IsSuccessStatusCode) return 0f;
        return await response.Content.ReadFromJsonAsync<float>();
    }

    public async Task<bool> AttemptExistsAsync(int userId, int testId)
    {
        var response = await http.GetAsync($"api/testattempts/byuser/{userId}/bytest/{testId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<TiAnswerDto>> GetAnswersByAttemptAsync(int attemptId)
    {
        var response = await http.GetAsync($"api/answers/byattempt/{attemptId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiAnswerDto>>() ?? new();
    }
}
