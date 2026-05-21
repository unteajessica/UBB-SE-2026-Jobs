using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Matches;

namespace PussyCats.Web.ServiceProxies;

public class MatchServiceProxy : IMatchService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient http;

    public MatchServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Match>> GetMatchesForUserAsync(int userId, CancellationToken cancellationToken = default)
        => await GetListAsync($"api/matches?userId={userId}", cancellationToken);

    public async Task<MatchStatistics> GetMatchStatisticsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/matches/statistics?userId={userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadRequiredAsync<MatchStatistics>(response, cancellationToken);
    }

    public async Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/matches/{matchId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Match>(JsonOptions, cancellationToken);
    }

    public async Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        var matches = await GetListAsync($"api/matches?userId={userId}&jobId={jobId}", cancellationToken);
        return matches.Count == 0 ? null : matches[0];
    }

    public async Task<IReadOnlyList<Match>> GetAllMatchesAsync(CancellationToken cancellationToken = default)
        => await GetListAsync("api/matches", cancellationToken);

    public async Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
        => await GetListAsync($"api/matches?companyId={companyId}", cancellationToken);

    public async Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/matches/{match.MatchId}", match, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<int> CreatePendingApplicationAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/matches", new { userId, jobId }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var match = await ReadRequiredAsync<Match>(response, cancellationToken);
        return match.MatchId;
    }

    public async Task RemoveApplicationAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/matches/{matchId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync(
            $"api/matches/{matchId}/decision",
            new { decision, feedback },
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task AcceptAsync(int matchId, string feedback, CancellationToken cancellationToken = default)
        => await SubmitDecisionAsync(matchId, MatchStatus.Accepted, feedback, cancellationToken);

    public async Task RejectAsync(int matchId, string feedback, CancellationToken cancellationToken = default)
        => await SubmitDecisionAsync(matchId, MatchStatus.Rejected, feedback, cancellationToken);

    public async Task AdvanceAsync(int matchId, CancellationToken cancellationToken = default)
        => await SendPatchWithoutBodyAsync($"api/matches/{matchId}/advance", cancellationToken);

    public async Task RevertToAppliedAsync(int matchId, CancellationToken cancellationToken = default)
        => await SendPatchWithoutBodyAsync($"api/matches/{matchId}/revert", cancellationToken);

    public bool IsDecisionTransitionAllowed(Match current, MatchStatus next)
        => MatchStatusTransitions.IsDecisionTransitionAllowed(current.Status, next);

    private async Task<IReadOnlyList<Match>> GetListAsync(string uri, CancellationToken cancellationToken)
        => await http.GetFromJsonAsync<List<Match>>(uri, JsonOptions, cancellationToken) ?? new List<Match>();

    private async Task SendPatchWithoutBodyAsync(string uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, uri);
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return value ?? throw new InvalidOperationException($"The API returned an empty {typeof(T).Name} response.");
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
