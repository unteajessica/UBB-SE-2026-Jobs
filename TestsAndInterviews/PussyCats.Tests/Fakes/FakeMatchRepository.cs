using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Matches;

namespace PussyCats.Tests.Fakes;

public class FakeMatchRepository : IMatchRepository
{
    private readonly Dictionary<int, Match> matchesById = new();

    public void Seed(params Match[] matches)
    {
        foreach (var match in matches)
        {
            matchesById[match.MatchId] = match;
        }
    }

    public Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        matchesById.TryGetValue(matchId, out var match);
        return Task.FromResult(match);
    }

    public Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Match> snapshot = matchesById.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<IReadOnlyList<Match>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {

        IReadOnlyList<Match> filtered = matchesById.Values.Where(match => match.User.UserId == userId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {

        var match = matchesById.Values.FirstOrDefault(match => match.User.UserId == userId && match.Job.JobId == jobId);
        return Task.FromResult(match);
    }

    public Task<Match> AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        if (match.MatchId == 0)
        {
            match.MatchId = NextId();
        }
        matchesById[match.MatchId] = match;
        return Task.FromResult(match);
    }

    public Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        matchesById[match.MatchId] = match;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int matchId, CancellationToken cancellationToken = default)
    {
        matchesById.Remove(matchId);
        return Task.CompletedTask;
    }

    private int NextId() => matchesById.Count == 0 ? 1 : matchesById.Keys.Max() + 1;
}
