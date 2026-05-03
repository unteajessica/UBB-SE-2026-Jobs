using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Matches;

namespace PussyCats.App.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository matchRepository;
    private readonly IJobService jobService;

    public MatchService(IMatchRepository matchRepository, IJobService jobService)
    {
        this.matchRepository = matchRepository;
        this.jobService = jobService;
    }

    public async Task<Match?> GetByIdAsync(int matchId, CancellationToken ct = default)
    {
        return await matchRepository.GetByIdAsync(matchId, ct).ConfigureAwait(false);
    }

    public async Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken ct = default)
    {
        return await matchRepository.GetByUserIdAndJobIdAsync(userId, jobId, ct).ConfigureAwait(false);
    }

    public async Task<int> CreatePendingApplicationAsync(int userId, int jobId, CancellationToken ct = default)
    {
        if (await GetByUserIdAndJobIdAsync(userId, jobId, ct).ConfigureAwait(false) is not null)
        {
            throw new InvalidOperationException("A match already exists for this user and job.");
        }

        var match = new Match
        {
            UserId = userId,
            JobId = jobId,
            Status = MatchStatus.Applied,
            Timestamp = DateTime.UtcNow,
            FeedbackMessage = string.Empty,
        };

        var saved = await matchRepository.AddAsync(match, ct).ConfigureAwait(false);
        return saved.MatchId;
    }

    public async Task RemoveApplicationAsync(int matchId, CancellationToken ct = default)
    {
        await matchRepository.RemoveAsync(matchId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetAllMatchesAsync(CancellationToken ct = default)
    {
        return await matchRepository.GetAllAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default)
    {
        var companyJobIds = new HashSet<int>();
        foreach (var job in await jobService.GetByCompanyIdAsync(companyId, ct).ConfigureAwait(false))
        {
            companyJobIds.Add(job.JobId);
        }

        if (companyJobIds.Count == 0)
        {
            return [];
        }

        var matches = new List<Match>();
        foreach (var match in await matchRepository.GetAllAsync(ct).ConfigureAwait(false))
        {
            if (companyJobIds.Contains(match.JobId))
            {
                matches.Add(match);
            }
        }

        matches.Sort(CompareByTimestampDescending);

        return matches;
    }

    public async Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback, CancellationToken ct = default)
    {
        var match = await matchRepository.GetByIdAsync(matchId, ct).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        ValidateDecisionInput(decision, feedback);

        if (!IsDecisionTransitionAllowed(match, decision))
        {
            throw new InvalidOperationException(
                $"Cannot change match {matchId} status from {match.Status} to {decision}.");
        }

        match.Status = decision;
        match.FeedbackMessage = feedback.Trim();
        match.Timestamp = DateTime.UtcNow;
        await matchRepository.UpdateAsync(match, ct).ConfigureAwait(false);
    }

    public async Task AcceptAsync(int matchId, string feedback, CancellationToken ct = default)
    {
        await SubmitDecisionAsync(matchId, MatchStatus.Accepted, feedback, ct).ConfigureAwait(false);
    }

    public async Task RejectAsync(int matchId, string feedback, CancellationToken ct = default)
    {
        await SubmitDecisionAsync(matchId, MatchStatus.Rejected, feedback, ct).ConfigureAwait(false);
    }

    public async Task AdvanceAsync(int matchId, CancellationToken ct = default)
    {
        var match = await matchRepository.GetByIdAsync(matchId, ct).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        if (match.Status != MatchStatus.Applied)
        {
            throw new InvalidOperationException(
                $"Cannot advance match {matchId}: status is {match.Status}, expected Applied.");
        }

        match.Status = MatchStatus.Advanced;
        match.Timestamp = DateTime.UtcNow;
        await matchRepository.UpdateAsync(match, ct).ConfigureAwait(false);
    }

    public async Task RevertToAppliedAsync(int matchId, CancellationToken ct = default)
    {
        var match = await matchRepository.GetByIdAsync(matchId, ct).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        match.Status = MatchStatus.Applied;
        match.FeedbackMessage = string.Empty;
        match.Timestamp = DateTime.UtcNow;
        await matchRepository.UpdateAsync(match, ct).ConfigureAwait(false);
    }

    public bool IsDecisionTransitionAllowed(Match current, MatchStatus next)
    {
        if (current.Status == MatchStatus.Applied)
        {
            return next is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;
        }

        if (current.Status == MatchStatus.Advanced)
        {
            return next is MatchStatus.Accepted or MatchStatus.Rejected;
        }

        return false;
    }

    private static void ValidateDecisionInput(MatchStatus decision, string feedback)
    {
        if (decision != MatchStatus.Accepted && decision != MatchStatus.Rejected)
        {
            throw new ArgumentException("Decision must be either Accepted or Rejected.", nameof(decision));
        }

        if (decision == MatchStatus.Rejected && string.IsNullOrWhiteSpace(feedback))
        {
            throw new ArgumentException("Feedback is required when rejecting an applicant.", nameof(feedback));
        }
    }

    private static int CompareByTimestampDescending(Match left, Match right)
    {
        return right.Timestamp.CompareTo(left.Timestamp);
    }
}
