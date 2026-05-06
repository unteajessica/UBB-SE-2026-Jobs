using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Matches;

namespace PussyCats.App.RepositoryProxies;

public class MatchRepositoryProxy : IMatchRepository
{
    private readonly HttpClient http;

    public MatchRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Match?> GetByIdAsync(int matchId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Match>(http, $"api/matches/{matchId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Match>(http, "api/matches", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Match>(http, $"api/matches?userId={userId}", ct).ConfigureAwait(false);
    }

    public async Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken ct = default)
    {
        var matches = await RepositoryProxyJson.GetListAsync<Match>(
            http,
            $"api/matches?userId={userId}&jobId={jobId}",
            ct).ConfigureAwait(false);
        return matches.Count == 0 ? null : matches[0];
    }

    public async Task<Match> AddAsync(Match match, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/matches", match, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Match>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Match match, CancellationToken ct = default)
    {
        var requestBody = new Match
        {
            MatchId = match.MatchId,
            UserId = match.UserId,
            JobId = match.JobId,
            Status = match.Status,
            Timestamp = match.Timestamp,
            FeedbackMessage = match.FeedbackMessage,
        };

        using var response = await http.PutAsJsonAsync($"api/matches/{match.MatchId}", requestBody, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int matchId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/matches/{matchId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
