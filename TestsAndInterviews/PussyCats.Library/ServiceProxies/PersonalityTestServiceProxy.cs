using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.PersonalityTestService;

namespace PussyCats.Library.ServiceProxies;

public class PersonalityTestServiceProxy : IPersonalityTestService
{
    private readonly HttpClient http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public PersonalityTestServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var response = await http
            .GetAsync($"api/personality-tests/users/{userId}", cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<PersonalityTestResult>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    public IReadOnlyDictionary<TraitType, double> CalculateTraitScores(IReadOnlyDictionary<Question, AnswerValue> personalityTestAnswers)
    {
        // Convertim Question -> QuestionText pentru a se potrivi cu DTO-ul din controller
        var payload = new
        {
            Answers = personalityTestAnswers.ToDictionary(
                answerQuestionPair => answerQuestionPair.Key.QuestionText,
                answerQuestionPair => answerQuestionPair.Value)
        };

        var response = http
            .PostAsJsonAsync("api/personality-tests/trait-scores", payload, JsonOptions)
            .GetAwaiter().GetResult();

        response.EnsureSuccessStatusCode();

        return response.Content
            .ReadFromJsonAsync<Dictionary<TraitType, double>>(JsonOptions)
            .GetAwaiter().GetResult()
            ?? new Dictionary<TraitType, double>();
    }

    public IReadOnlyDictionary<JobRole, double> CalculateRoleScores(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        var response = http
            .PostAsJsonAsync("api/personality-tests/role-scores", traitScores, JsonOptions)
            .GetAwaiter().GetResult();

        response.EnsureSuccessStatusCode();

        return response.Content
            .ReadFromJsonAsync<Dictionary<JobRole, double>>(JsonOptions)
            .GetAwaiter().GetResult()
            ?? new Dictionary<JobRole, double>();
    }

    public IReadOnlyDictionary<JobRole, double> GetTopRoles(IReadOnlyDictionary<JobRole, double> roleScores, int count)
    {
        var payload = new { RoleScores = roleScores, Count = count };

        var response = http
            .PostAsJsonAsync("api/personality-tests/role-scores/top", payload, JsonOptions)
            .GetAwaiter().GetResult();

        response.EnsureSuccessStatusCode();

        return response.Content
            .ReadFromJsonAsync<Dictionary<JobRole, double>>(JsonOptions)
            .GetAwaiter().GetResult()
            ?? new Dictionary<JobRole, double>();
    }

    public async Task SaveResultAsync(int userId, IReadOnlyDictionary<Question, AnswerValue> answers, JobRole selectedRole, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            Answers = answers.ToDictionary(
                answerQuestionPair => answerQuestionPair.Key.QuestionText,
                answerQuestionPair => answerQuestionPair.Value),
            SelectedRole = selectedRole,
        };

        var response = await http
            .PostAsJsonAsync($"api/personality-tests/users/{userId}", payload, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }
}