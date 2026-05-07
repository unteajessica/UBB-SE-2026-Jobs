using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

public interface IMatchService
{
    Task<IReadOnlyList<Match>> GetMatchesForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<MatchStatistics> GetMatchStatisticsAsync(int userId, CancellationToken cancellationToken = default);

    Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default);

    Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Match>> GetAllMatchesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default);

    Task<int> CreatePendingApplicationAsync(int userId, int jobId, CancellationToken cancellationToken = default);

    Task RemoveApplicationAsync(int matchId, CancellationToken cancellationToken = default);

    Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback, CancellationToken cancellationToken = default);

    Task AcceptAsync(int matchId, string feedback, CancellationToken cancellationToken = default);

    Task RejectAsync(int matchId, string feedback, CancellationToken cancellationToken = default);

    Task AdvanceAsync(int matchId, CancellationToken cancellationToken = default);

    Task RevertToAppliedAsync(int matchId, CancellationToken cancellationToken = default);

    bool IsDecisionTransitionAllowed(Match current, MatchStatus next);
}
