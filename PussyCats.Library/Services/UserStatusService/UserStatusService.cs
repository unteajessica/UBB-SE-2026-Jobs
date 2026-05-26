using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Services.CompanyService;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.JobSkills;
using PussyCats.Library.Services.UserSkillService;

namespace PussyCats.Library.Services.UserStatusService;

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

        var jobsById = (await jobService.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(job => job.JobId);
        var companiesById = (await companyService.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(company => company.CompanyId);
        var jobSkillsByJobId = (await jobSkillService.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .GroupBy(jobSkillWithId => jobSkillWithId.Job.JobId)
            .ToDictionary(groupDictionary => groupDictionary.Key, groupDictionary => (IReadOnlyList<JobSkill>)groupDictionary.ToList());

        var result = new List<ApplicationCardModel>();
        foreach (var match in matches)
        {
            if (!jobsById.TryGetValue(match.Job.JobId, out var job))
                continue;

            companiesById.TryGetValue(job.Company.CompanyId, out var company);
            var jobSkills = jobSkillsByJobId.GetValueOrDefault(match.Job.JobId) ?? [];
            var score = CalculateCompatibilityScore(userSkills, jobSkills);

            result.Add(new ApplicationCardModel
            {
                MatchId = match.MatchId,
                JobId = match.Job.JobId,
                CompanyName = company?.CompanyName ?? "Unknown Company",
                JobDescription = job.JobDescription,
                AppliedDate = match.Timestamp,
                Status = match.Status,
                CompatibilityScore = score,
                FeedbackMessage = match.FeedbackMessage,
            });
        }

        return result;
    }

    private static int CalculateCompatibilityScore(IReadOnlyList<UserSkill> userSkills, IReadOnlyList<JobSkill> jobSkills)
    {
        if (jobSkills.Count == 0)
            return 100;

        var userSkillMap = userSkills.ToDictionary(userSkillToAddToMap => userSkillToAddToMap.Skill.SkillId, userSkill => userSkill.Score);

        double total = 0;
        foreach (var required in jobSkills)
        {
            if (userSkillMap.TryGetValue(required.Skill.SkillId, out var userScore))
                total += Math.Min(userScore, required.RequiredLevel) / (double)required.RequiredLevel;
        }

        return (int)(total / jobSkills.Count * 100);
    }
}
