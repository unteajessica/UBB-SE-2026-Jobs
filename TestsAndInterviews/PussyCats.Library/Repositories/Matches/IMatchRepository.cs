using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Matches;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Match>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default);

    Task<Match> AddAsync(Match match, CancellationToken cancellationToken = default);

    Task UpdateAsync(Match match, CancellationToken cancellationToken = default);

    Task RemoveAsync(int matchId, CancellationToken cancellationToken = default);
}
