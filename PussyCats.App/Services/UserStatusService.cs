using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Matches;

namespace PussyCats.App.Services;

public class UserStatusService : IUserStatusService
{
    private readonly IMatchRepository matchRepository;
    private readonly IJobService jobService;
    private readonly ICompanyService companyService;
    private readonly IUserSkillService userSkillService;
    private readonly IJobSkillService jobSkillService;

    public UserStatusService(
        IMatchRepository matchRepository,
        IJobService jobService,
        ICompanyService companyService,
        IUserSkillService userSkillService,
        IJobSkillService jobSkillService)
    {
        this.matchRepository = matchRepository;
        this.jobService = jobService;
        this.companyService = companyService;
        this.userSkillService = userSkillService;
        this.jobSkillService = jobSkillService;
    }

    public async Task<IReadOnlyList<ApplicationCardModel>> GetApplicationsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var matches = await matchRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var userSkills = await userSkillService.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var result = new List<ApplicationCardModel>();

        foreach (var match in matches)
        {
            var matchedJob = await jobService.GetByIdAsync(match.JobId, cancellationToken).ConfigureAwait(false);
            if (matchedJob is null)
            {
                continue;
            }

            var company = await companyService.GetByIdAsync(matchedJob.CompanyId, cancellationToken).ConfigureAwait(false);
            var jobSkills = await jobSkillService.GetByJobIdAsync(match.JobId, cancellationToken).ConfigureAwait(false);
            var score = CalculateCompatibilityScore(userSkills, jobSkills);

            result.Add(new ApplicationCardModel
            {
                MatchId = match.MatchId,
                JobId = match.JobId,
                CompanyName = company?.CompanyName ?? "Unknown Company",
                JobDescription = matchedJob.JobDescription,
                AppliedDate = match.Timestamp,
                Status = match.Status,
                CompatibilityScore = score,
                FeedbackMessage = match.FeedbackMessage,
            });
        }

        return result;
    }

    private static int CalculateCompatibilityScore(
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills)
    {
        if (jobSkills.Count == 0)
        {
            return 100;
        }

        var userSkillMap = new Dictionary<int, int>();
        foreach (var userSkill in userSkills)
        {
            userSkillMap[userSkill.SkillId] = userSkill.Score;
        }

        double total = 0;
        foreach (var required in jobSkills)
        {
            if (userSkillMap.TryGetValue(required.SkillId, out var userScore))
            {
                total += Math.Min(userScore, required.RequiredLevel) / (double)required.RequiredLevel;
            }
        }

        return (int)(total / jobSkills.Count * 100);
    }
}
