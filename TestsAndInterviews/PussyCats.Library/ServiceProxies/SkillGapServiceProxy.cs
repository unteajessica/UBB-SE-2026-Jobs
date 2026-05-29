using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.SkillGapService;

namespace PussyCats.Library.ServiceProxies;

public class SkillGapServiceProxy : ISkillGapService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient http;

    public SkillGapServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<MissingSkillModel>> GetMissingSkillsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var response = await GetSkillGapAsync(userId, cancellationToken).ConfigureAwait(false);
        return response.MissingSkills;
    }

    public async Task<IReadOnlyList<UnderscoredSkillModel>> GetUnderscoredSkillsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var response = await GetSkillGapAsync(userId, cancellationToken).ConfigureAwait(false);
        return response.UnderscoredSkills;
    }

    public async Task<SkillGapSummaryModel> GetSummaryAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var response = await GetSkillGapAsync(userId, cancellationToken).ConfigureAwait(false);
        return response.Summary;
    }

    private async Task<SkillGapResponse> GetSkillGapAsync(int userId, CancellationToken cancellationToken)
    {
        return await http.GetFromJsonAsync<SkillGapResponse>(
                $"api/users/{userId}/skill-gap",
                JsonOptions,
                cancellationToken)
            .ConfigureAwait(false)
            ?? new SkillGapResponse(
                new SkillGapSummaryModel(),
                Array.Empty<MissingSkillModel>(),
                Array.Empty<UnderscoredSkillModel>());
    }

    private sealed record SkillGapResponse(
        SkillGapSummaryModel Summary,
        IReadOnlyList<MissingSkillModel> MissingSkills,
        IReadOnlyList<UnderscoredSkillModel> UnderscoredSkills);
}
