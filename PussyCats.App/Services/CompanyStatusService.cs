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
    private const int LocationBonusPoints = 10;
    private const int EmploymentTypeBonusPoints = 10;
    private const double MaxCompatibilityScore = 100;

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
            var user = await userService.GetByIdAsync(match.User.UserId, cancellationToken).ConfigureAwait(false);

            if(user is null)
            {
                continue;
            }

            var job = await jobService.GetByIdAsync(match.Job.JobId, cancellationToken).ConfigureAwait(false);
            if (job is null)
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

        var locationBonus = LocationsReferToSameCity(user.City, job.Location) ? LocationBonusPoints : 0;
        var employmentTypeBonus = user.PreferredEmploymentType.Equals(job.EmploymentType, StringComparison.OrdinalIgnoreCase)
            ? EmploymentTypeBonusPoints
            : 0;

        var computed = averageSkillScore + locationBonus + employmentTypeBonus;
        return computed > MaxCompatibilityScore ? MaxCompatibilityScore : computed;
    }

    private static bool LocationsReferToSameCity(string userCity, string jobLocation)
    {
        var normalizedCity = NormalizeLocationToken(userCity);
        if (normalizedCity.Length == 0)
        {
            return false;
        }

        foreach (var token in jobLocation.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (NormalizeLocationToken(token) == normalizedCity)
            {
                return true;
            }
        }

        return NormalizeLocationToken(jobLocation) == normalizedCity;
    }

    private static string NormalizeLocationToken(string value)
    {
        return string.Join(
            ' ',
            value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool IsVisibleMatch(Match match)
    {
        return match.Status is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;
    }

    private static int CompareByCompatibilityScoreDescending(UserApplicationResult firstApplicant, UserApplicationResult secondApplicant)
    {
        return secondApplicant.CompatibilityScore.CompareTo(firstApplicant.CompatibilityScore);
    }

    private static double ComputeAverageSkillScore(IReadOnlyList<UserSkill> userSkills)
    {
        var totalSkillScore = 0;
        foreach (var userSkill in userSkills)
        {
            totalSkillScore += userSkill.Score;
        }

        return (double)totalSkillScore / userSkills.Count;
    }
}
