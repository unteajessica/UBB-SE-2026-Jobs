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

    public async Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Match>(http, $"api/matches/{matchId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Match>(http, "api/matches", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Match>(http, $"api/matches?userId={userId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        var matches = await RepositoryProxyJson.GetListAsync<Match>(
            http,
            $"api/matches?userId={userId}&jobId={jobId}",
            cancellationToken).ConfigureAwait(false);
        return matches.Count == 0 ? null : matches[0];
    }

    public async Task<Match> AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/matches",
            new { UserId = match.User.UserId, JobId = match.Job.JobId },
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Match>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        var requestBody = new Match
        {
            MatchId = match.MatchId,
            User = match.User,
            Job = match.Job,
            Status = match.Status,
            Timestamp = match.Timestamp,
            FeedbackMessage = match.FeedbackMessage,
        };

        using var response = await http.PutAsJsonAsync($"api/matches/{match.MatchId}", requestBody, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int matchId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/matches/{matchId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
