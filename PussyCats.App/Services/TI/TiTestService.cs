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
        var attempt = await GetAttemptByUserAndTestAsync(userId, testId);
        if (attempt == null) return 0f;

        var questions = await GetQuestionsByTestIdAsync(testId);
        var questionMap = questions.ToDictionary(q => q.Id);
        float maxPossibleScore = questions.Sum(q => q.QuestionScore);

        var gradedAnswers = new List<TiAnswerDto>();

        foreach (var answer in answers)
        {
            if (!questionMap.TryGetValue(answer.QuestionId, out var question)) continue;

            string endpoint = question.QuestionType switch
            {
                "SINGLE_CHOICE" => "single-choice",
                "MULTIPLE_CHOICE" => "multiple-choice",
                "TRUE_FALSE" => "true-false",
                _ => "text"
            };

            var gradeRequest = new
            {
                Question = new
                {
                    Id = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionTypeString = question.QuestionType,
                    QuestionScore = question.QuestionScore,
                    QuestionAnswer = question.QuestionAnswer,
                },
                Answer = new
                {
                    QuestionId = question.Id,
                    AttemptId = attempt.Id,
                    Value = answer.Value,
                }
            };

            var gradeResponse = await http.PostAsJsonAsync($"api/grading/{endpoint}", gradeRequest);
            if (!gradeResponse.IsSuccessStatusCode) continue;

            var gradedAnswer = await gradeResponse.Content.ReadFromJsonAsync<TiAnswerDto>();
            if (gradedAnswer == null) continue;

            gradedAnswers.Add(gradedAnswer);
            await http.PostAsJsonAsync("api/answers", gradedAnswer);
        }

        var scorePayload = new
        {
            Id = attempt.Id,
            Answers = gradedAnswers.Select(a => new { Value = a.Value }).ToList()
        };

        var scoreResponse = await http.PostAsJsonAsync("api/grading/final-score", scorePayload);
        float rawScore = scoreResponse.IsSuccessStatusCode
            ? await scoreResponse.Content.ReadFromJsonAsync<float>()
            : 0f;

        attempt.Status = "COMPLETED";
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.Score = (decimal)rawScore;
        if (maxPossibleScore > 0)
            attempt.PercentageScore = (decimal)(rawScore / maxPossibleScore * 100f);

        await http.PutAsJsonAsync($"api/testattempts/{attempt.Id}", attempt);
        await http.PostAsync($"api/leaderboard/recalculate/{testId}", null);

        return rawScore;
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
