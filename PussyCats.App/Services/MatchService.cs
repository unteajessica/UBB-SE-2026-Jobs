using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Matches;

namespace PussyCats.App.Services;

public class MatchService : IMatchService
{
    private const int LastMonth = 1;
    private const int LastSixMonths = 6;
    private const int LastYear = 12;


    private readonly IMatchRepository matchRepository;
    private readonly IJobService jobService;
    private readonly IUserService userService;

    public MatchService(IMatchRepository matchRepository, IJobService jobService, IUserService userService)
    {
        this.matchRepository = matchRepository;
        this.jobService = jobService;
        this.userService = userService;
    }

    public async Task<IReadOnlyList<Match>> GetMatchesForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await matchRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MatchStatistics> GetMatchStatisticsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var matchStatistics = new MatchStatistics();

        matchStatistics.TotalMatches = matches.Count;
        matchStatistics.MatchesLastMonth = CountMatchesInLastMonths(matches, LastMonth);
        matchStatistics.MatchesLastSixMonths = CountMatchesInLastMonths(matches, LastSixMonths);
        matchStatistics.MatchesLastYear = CountMatchesInLastMonths(matches, LastYear);
        matchStatistics.MatchesPerPosition = GroupMatchesByPosition(matches);

        return matchStatistics;
    }

    public async Task<Match?> GetByIdAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return await matchRepository.GetByIdAsync(matchId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Match?> GetByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        return await matchRepository.GetByUserIdAndJobIdAsync(userId, jobId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> CreatePendingApplicationAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        if (await GetByUserIdAndJobIdAsync(userId, jobId, cancellationToken).ConfigureAwait(false) is not null)
        {
            throw new InvalidOperationException("A match already exists for this user and job.");
        }

        var user = await userService.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        var job = await jobService.GetByIdAsync(jobId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Job {jobId} not found.");

        var match = new Match
        {
            User = user,
            Job = job,
            Status = MatchStatus.Applied,
            Timestamp = DateTime.UtcNow,
            FeedbackMessage = string.Empty,
        };

        var saved = await matchRepository.AddAsync(match, cancellationToken).ConfigureAwait(false);
        return saved.MatchId;
    }

    public async Task RemoveApplicationAsync(int matchId, CancellationToken cancellationToken = default)
    {
        await matchRepository.RemoveAsync(matchId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetAllMatchesAsync(CancellationToken cancellationToken = default)
    {
        return await matchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var companyJobIds = new HashSet<int>();
        foreach (var job in await jobService.GetByCompanyIdAsync(companyId, cancellationToken).ConfigureAwait(false))
        {
            companyJobIds.Add(job.JobId);
        }

        if (companyJobIds.Count == 0)
        {
            return [];
        }

        var matches = new List<Match>();
        foreach (var match in await matchRepository.GetAllAsync(cancellationToken).ConfigureAwait(false))
        {
            if (companyJobIds.Contains(match.Job.JobId))
            {
                matches.Add(match);
            }
        }

        matches.Sort(CompareByTimestampDescending);

        return matches;
    }

    public async Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback, CancellationToken cancellationToken = default)
    {
        var match = await matchRepository.GetByIdAsync(matchId, cancellationToken).ConfigureAwait(false)
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
        await matchRepository.UpdateAsync(match, cancellationToken).ConfigureAwait(false);
    }

    public async Task AcceptAsync(int matchId, string feedback, CancellationToken cancellationToken = default)
    {
        await SubmitDecisionAsync(matchId, MatchStatus.Accepted, feedback, cancellationToken).ConfigureAwait(false);
    }

    public async Task RejectAsync(int matchId, string feedback, CancellationToken cancellationToken = default)
    {
        await SubmitDecisionAsync(matchId, MatchStatus.Rejected, feedback, cancellationToken).ConfigureAwait(false);
    }

    public async Task AdvanceAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var match = await matchRepository.GetByIdAsync(matchId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        if (match.Status != MatchStatus.Applied)
        {
            throw new InvalidOperationException(
                $"Cannot advance match {matchId}: status is {match.Status}, expected Applied.");
        }

        match.Status = MatchStatus.Advanced;
        match.Timestamp = DateTime.UtcNow;
        await matchRepository.UpdateAsync(match, cancellationToken).ConfigureAwait(false);
    }

    public async Task RevertToAppliedAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var match = await matchRepository.GetByIdAsync(matchId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Match with id {matchId} was not found.");

        match.Status = MatchStatus.Applied;
        match.FeedbackMessage = string.Empty;
        match.Timestamp = DateTime.UtcNow;
        await matchRepository.UpdateAsync(match, cancellationToken).ConfigureAwait(false);
    }

    public bool IsDecisionTransitionAllowed(Match current, MatchStatus next)
    {
        return MatchStatusTransitions.IsDecisionTransitionAllowed(current.Status, next);
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

    private static int CountMatchesInLastMonths(IReadOnlyList<Match> matches, int months)
    {
        DateTime cutoffDate = DateTime.Now.AddMonths(-months);
        int matchesInPeriodCount = 0;

        foreach (var match in matches)
        {
            if (match.Timestamp > cutoffDate)
            {
                matchesInPeriodCount++;
            }
        }

        return matchesInPeriodCount;
    }

    private static Dictionary<string, int> GroupMatchesByPosition(IReadOnlyList<Match> matches)
    {
        var positionCounts = new Dictionary<string, int>();

        foreach (var match in matches)
        {
            if (match.Job is null)
            {
                continue;
            }

            string position = GetPositionKey(match.Job.JobRole);

            if (positionCounts.ContainsKey(position))
            {
                positionCounts[position]++;
            }
            else
            {
                positionCounts.Add(position, 1);
            }
        }

        return positionCounts;
    }

    // Display labels for the MatchesPerPosition stats card. Original
    // PussyCatsApp stored free-form strings in the DB; the merged model
    // uses the JobRole enum as canonical source. These labels are what
    // the UI shows. Update here if Phase 6 design wants different wording.
    private static string GetPositionKey(JobRole role) => role switch
    {
        JobRole.FrontendDeveloper        => "Frontend",
        JobRole.BackendDeveloper         => "Backend",
        JobRole.UiUxDesigner             => "UI/UX Design",
        JobRole.DevOpsEngineer           => "DevOps",
        JobRole.ProjectManager           => "Project Management",
        JobRole.DataAnalyst              => "Data Analysis",
        JobRole.CybersecuritySpecialist  => "Cybersecurity",
        JobRole.AiMlEngineer             => "AI/ML Engineering",
        _                                => role.ToString(),
    };
}
