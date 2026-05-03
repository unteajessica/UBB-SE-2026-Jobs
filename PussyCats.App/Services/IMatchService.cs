using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.Services;

public interface IMatchService
{
    Task<Match?> GetByIdAsync(int matchId, CancellationToken ct = default);

    Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken ct = default);

    Task<IReadOnlyList<Match>> GetAllMatchesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default);

    Task<int> CreatePendingApplicationAsync(int userId, int jobId, CancellationToken ct = default);

    Task RemoveApplicationAsync(int matchId, CancellationToken ct = default);

    Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback, CancellationToken ct = default);

    Task AcceptAsync(int matchId, string feedback, CancellationToken ct = default);

    Task RejectAsync(int matchId, string feedback, CancellationToken ct = default);

    Task AdvanceAsync(int matchId, CancellationToken ct = default);

    Task RevertToAppliedAsync(int matchId, CancellationToken ct = default);

    bool IsDecisionTransitionAllowed(Match current, MatchStatus next);
}
