using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.PersonalityTestService;

namespace PussyCats.Web.ServiceProxies;

public class PersonalityTestServiceProxy : IPersonalityTestService
{
    private readonly HttpClient http;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters = { new JsonStringEnumConverter() }
    };

    public PersonalityTestServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/personality-tests?userId={userId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PersonalityTestResult>(JsonOptions, cancellationToken);
    }

    public async Task SaveResultAsync(int userId, IReadOnlyDictionary<Question, AnswerValue> answers, JobRole selectedRole, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            UserId = userId,
            SelectedRole = selectedRole,
            Answers = answers.Select(kv => new
            {
                QuestionText = kv.Key.QuestionText,
                Trait = kv.Key.Trait,
                SortOrder = kv.Key.SortOrder,
                Answer = (int)kv.Value,
            }).ToList(),
        };

        var response = await http.PostAsJsonAsync("api/personality-tests", payload, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
    public async Task<IReadOnlyDictionary<JobRole, double>> CalculateAsync(int userId,IReadOnlyDictionary<Question, AnswerValue> answers, CancellationToken ct = default)
    {

        var payload = new
        {
            UserId = userId,
            SelectedRole = 0,
            Answers = answers.Select(kv => new
            {
                QuestionText = kv.Key.QuestionText,
                Trait = kv.Key.Trait,
                SortOrder = kv.Key.SortOrder,
                Answer = (int)kv.Value
            }).ToList()
        };

        var response = await http.PostAsJsonAsync(
            "api/personality-tests/calculate", payload, JsonOptions, ct);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<Dictionary<JobRole, double>>(JsonOptions, cancellationToken: ct))!;
    }
    
    public IReadOnlyDictionary<TraitType, double> CalculateTraitScores(IReadOnlyDictionary<Question, AnswerValue> answers)
        => throw new NotSupportedException();

    public IReadOnlyDictionary<JobRole, double> CalculateRoleScores(IReadOnlyDictionary<TraitType, double> traitScores)
        => throw new NotSupportedException();

    public IReadOnlyDictionary<JobRole, double> GetTopRoles(IReadOnlyDictionary<JobRole, double> roleScores, int count)
        => throw new NotSupportedException();
}