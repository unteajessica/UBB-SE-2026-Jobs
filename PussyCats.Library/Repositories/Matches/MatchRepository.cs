using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Matches;

public class MatchRepository : IMatchRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public MatchRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Includes User and Job (with Company) — recruiters viewing a match need both sides.
    /// Tracked because MatchService.SubmitDecision mutates.
    /// </summary>
    public async Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Matches
            .Include(match => match.User)
            .Include(match => match.Job).ThenInclude(job => job.Company)
            .FirstOrDefaultAsync(match => match.MatchId == matchId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Matches
            .AsNoTracking()
            .Include(match => match.Job).ThenInclude(job => job.Company)
            .Include(match=>match.User)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp MatchRepository.GetMatchesByUserId — preserves the
    /// "ORDER BY matchDate DESC" ordering. Read-only, includes Job/Company so the My Applications
    /// list can render without N+1.
    /// </summary>
    public async Task<IReadOnlyList<Match>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Matches
            .AsNoTracking()
            .Include(match => match.User)
            .Where(match => match.User.UserId == userId)
            .Include(match => match.Job).ThenInclude(job => job.Company)
            .OrderByDescending(match => match.Timestamp)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking SqlMatchRepository.GetByUserIdAndJobId. LINQ translation of the
    /// raw "WHERE UserID = @UserId AND JobID = @JobId" — same predicate, no extra checks.
    /// </summary>
    public async Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Matches
            .Include(match => match.User)
            .FirstOrDefaultAsync(match => match.User.UserId == userId && match.JobId == jobId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Match> AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        if (match.Timestamp == default)
        {
            match.Timestamp = DateTime.UtcNow;
        }
        databaseContext.Matches.Add(match);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return match;
    }

    public async Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.Matches.Local.FirstOrDefault(existing => existing.MatchId == match.MatchId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(match);
        }
        else
        {
            databaseContext.Entry(match).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var match = await databaseContext.Matches.FindAsync(new object?[] { matchId }, cancellationToken).ConfigureAwait(false);
        if (match is null)
        {
            return;
        }
        databaseContext.Matches.Remove(match);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
