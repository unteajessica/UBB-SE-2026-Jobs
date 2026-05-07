using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

// Holds per-session state (queue, currentIndex). Must be registered as
// Transient or per-view-model in DI — see Phase 5. Sharing across users
// would leak applicants between sessions.
public class CompanyRecommendationService : ICompanyRecommendationService
{
    private readonly IMatchService matchService;
    private readonly IUserService userService;
    private readonly IJobService jobService;
    private readonly IUserSkillService userSkillService;
    private readonly IJobSkillService jobSkillService;
    private readonly IRecommendationAlgorithm algorithm;

    private List<UserApplicationResult> queue = new List<UserApplicationResult>();
    private int currentIndex;

    public CompanyRecommendationService(
        IMatchService matchService,
        IUserService userService,
        IJobService jobService,
        IUserSkillService userSkillService,
        IJobSkillService jobSkillService,
        IRecommendationAlgorithm algorithm)
    {
        this.matchService = matchService;
        this.userService = userService;
        this.jobService = jobService;
        this.userSkillService = userSkillService;
        this.jobSkillService = jobSkillService;
        this.algorithm = algorithm;
    }

    public async Task LoadApplicantsAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var companyJobs = await jobService.GetByCompanyIdAsync(companyId, cancellationToken).ConfigureAwait(false);
        var companyJobIds = GetJobIds(companyJobs);

        if (companyJobIds.Count == 0)
        {
            queue = new List<UserApplicationResult>();
            currentIndex = 0;
            return;
        }

        var allMatches = await matchService.GetAllMatchesAsync(cancellationToken).ConfigureAwait(false);
        var appliedMatches = new List<Match>();
        foreach (var match in allMatches)
        {
            if (match.Status == MatchStatus.Applied && companyJobIds.Contains(match.JobId))
            {
                appliedMatches.Add(match);
            }
        }

        var results = new List<UserApplicationResult>();
        foreach (var match in appliedMatches)
        {
            var user = await userService.GetByIdAsync(match.UserId, cancellationToken).ConfigureAwait(false);
            var job = await jobService.GetByIdAsync(match.JobId, cancellationToken).ConfigureAwait(false);
            if (user is null || job is null)
            {
                continue;
            }

            var userSkills = await userSkillService.GetByUserIdAsync(user.UserId, cancellationToken).ConfigureAwait(false);
            var jobSkills = await jobSkillService.GetByJobIdAsync(job.JobId, cancellationToken).ConfigureAwait(false);

            var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

            results.Add(new UserApplicationResult
            {
                User = user,
                Match = match,
                Job = job,
                CompatibilityScore = score,
                UserSkills = userSkills,
                Feedback = match.FeedbackMessage,
            });
        }

        results.Sort(CompareByCompatibilityScoreDescending);
        queue = results;
        currentIndex = 0;
    }

    public UserApplicationResult? GetNextApplicant()
    {
        if (currentIndex >= queue.Count)
        {
            return null;
        }

        return queue[currentIndex];
    }

    public void MoveToNext()
    {
        currentIndex++;
    }

    public void MoveToPrevious()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
        }
    }

    public bool HasMore => currentIndex < queue.Count;

    public async Task<CompatibilityBreakdown?> GetBreakdownAsync(UserApplicationResult applicant, CancellationToken cancellationToken = default)
    {
        var jobSkills = await jobSkillService.GetByJobIdAsync(applicant.Job.JobId, cancellationToken).ConfigureAwait(false);

        return algorithm.CalculateScoreBreakdown(
            applicant.User,
            applicant.Job,
            applicant.UserSkills,
            jobSkills);
    }

    private static HashSet<int> GetJobIds(IReadOnlyList<Job> companyJobs)
    {
        var jobIds = new HashSet<int>();
        foreach (var job in companyJobs)
        {
            jobIds.Add(job.JobId);
        }

        return jobIds;
    }

    private static int CompareByCompatibilityScoreDescending(UserApplicationResult left, UserApplicationResult right)
    {
        return right.CompatibilityScore.CompareTo(left.CompatibilityScore);
    }
}
