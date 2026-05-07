using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

public class CompanyStatusService : ICompanyStatusService
{
    private readonly IMatchService matchService;
    private readonly IUserService userService;
    private readonly IJobService jobService;
    private readonly IUserSkillService userSkillService;

    public CompanyStatusService(
        IMatchService matchService,
        IUserService userService,
        IJobService jobService,
        IUserSkillService userSkillService)
    {
        this.matchService = matchService;
        this.userService = userService;
        this.jobService = jobService;
        this.userSkillService = userSkillService;
    }

    public async Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var matches = await matchService.GetByCompanyIdAsync(companyId, cancellationToken).ConfigureAwait(false);
        var visibleMatches = new List<Match>();
        foreach (var match in matches)
        {
            if (IsVisibleMatch(match))
            {
                visibleMatches.Add(match);
            }
        }

        var results = new List<UserApplicationResult>(visibleMatches.Count);

        foreach (var match in visibleMatches)
        {
            var user = await userService.GetByIdAsync(match.UserId, cancellationToken).ConfigureAwait(false);
            var job = await jobService.GetByIdAsync(match.JobId, cancellationToken).ConfigureAwait(false);
            if (user is null || job is null)
            {
                continue;
            }

            var userSkills = await userSkillService.GetByUserIdAsync(user.UserId, cancellationToken).ConfigureAwait(false);
            var result = BuildResult(match, user, job, userSkills);
            results.Add(result);
        }

        results.Sort(CompareByCompatibilityScoreDescending);

        return results;
    }

    public async Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId, CancellationToken cancellationToken = default)
    {
        var applicants = await GetApplicantsForCompanyAsync(companyId, cancellationToken).ConfigureAwait(false);
        foreach (var result in applicants)
        {
            if (result.Match.MatchId == matchId)
            {
                return result;
            }
        }

        return null;
    }

    private static UserApplicationResult BuildResult(
        Match match,
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills)
    {
        return new UserApplicationResult
        {
            User = user,
            Match = match,
            Job = job,
            CompatibilityScore = ComputeCompatibilityFallback(user, job, userSkills),
            UserSkills = userSkills,
            Feedback = match.FeedbackMessage,
        };
    }

    private static double ComputeCompatibilityFallback(User user, Job job, IReadOnlyList<UserSkill> userSkills)
    {
        if (userSkills.Count == 0)
        {
            return 0;
        }

        var averageSkillScore = ComputeAverageSkillScore(userSkills);

        // TODO (Phase 6): User.City may not match Job.Location formats. User.City
        // stores bare city names ("Bucharest") while Job.Location may include
        // country ("Bucharest, Romania") depending on how the company entered
        // it — equality returns false even when the locations are the same place.
        // Original matchmaking code compared User.Location vs Job.Location, both
        // single-column city strings; merged User has no Location property so
        // User.City is the closest substitute. Add format normalization here
        // before relying on locationBonus for ranking.
        var locationBonus = user.City.Equals(job.Location, StringComparison.OrdinalIgnoreCase) ? 10 : 0;
        var employmentTypeBonus = user.PreferredEmploymentType.Equals(job.EmploymentType, StringComparison.OrdinalIgnoreCase)
            ? 10
            : 0;

        var computed = averageSkillScore + locationBonus + employmentTypeBonus;
        return computed > 100 ? 100 : computed;
    }

    private static bool IsVisibleMatch(Match match)
    {
        return match.Status is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;
    }

    private static int CompareByCompatibilityScoreDescending(UserApplicationResult left, UserApplicationResult right)
    {
        return right.CompatibilityScore.CompareTo(left.CompatibilityScore);
    }

    private static double ComputeAverageSkillScore(IReadOnlyList<UserSkill> userSkills)
    {
        var sum = 0;
        foreach (var userSkill in userSkills)
        {
            sum += userSkill.Score;
        }

        return (double)sum / userSkills.Count;
    }
}
