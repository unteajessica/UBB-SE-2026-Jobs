using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Matches;

public class MatchRepository : IMatchRepository
{
    private readonly PussyCatsDbContext db;

    public MatchRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Includes User and Job (with Company) — recruiters viewing a match need both sides.
    /// Tracked because MatchService.SubmitDecision mutates.
    /// </summary>
    public async Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return await db.Matches
            .Include(m => m.User)
            .Include(m => m.Job).ThenInclude(job => job.Company)
            .FirstOrDefaultAsync(m => m.MatchId == matchId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Matches
            .AsNoTracking()
            .Include(m => m.Job).ThenInclude(job => job.Company)
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
        return await db.Matches
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Include(m => m.Job).ThenInclude(job => job.Company)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking SqlMatchRepository.GetByUserIdAndJobId. LINQ translation of the
    /// raw "WHERE UserID = @UserId AND JobID = @JobId" — same predicate, no extra checks.
    /// </summary>
    public async Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        return await db.Matches
            .FirstOrDefaultAsync(m => m.UserId == userId && m.JobId == jobId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Match> AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        if (match.Timestamp == default)
        {
            match.Timestamp = DateTime.UtcNow;
        }
        db.Matches.Add(match);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return match;
    }

    public async Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        db.Matches.Update(match);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var match = await db.Matches.FindAsync(new object?[] { matchId }, cancellationToken).ConfigureAwait(false);
        if (match is null)
        {
            return;
        }
        db.Matches.Remove(match);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
