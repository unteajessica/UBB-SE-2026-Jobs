using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Recommendations;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.Users;
using PussyCats.Library.Services;

namespace PussyCats.App.Services;

public sealed class UserRecommendationService : IUserRecommendationService
{
    private readonly IUserRepository userRepository;
    private readonly IJobRepository jobRepository;
    private readonly IUserSkillRepository userSkillRepository;
    private readonly IJobSkillRepository jobSkillRepository;
    private readonly ICompanyRepository companyRepository;
    private readonly IMatchService matchService;
    private readonly IRecommendationRepository recommendationRepository;
    private readonly ICooldownService cooldownService;
    private readonly IRecommendationAlgorithm algorithm;

    public UserRecommendationService(
        IUserRepository userRepository,
        IJobRepository jobRepository,
        IUserSkillRepository userSkillRepository,
        IJobSkillRepository jobSkillRepository,
        ICompanyRepository companyRepository,
        IMatchService matchService,
        IRecommendationRepository recommendationRepository,
        ICooldownService cooldownService,
        IRecommendationAlgorithm algorithm)
    {
        this.userRepository = userRepository;
        this.jobRepository = jobRepository;
        this.userSkillRepository = userSkillRepository;
        this.jobSkillRepository = jobSkillRepository;
        this.companyRepository = companyRepository;
        this.matchService = matchService;
        this.recommendationRepository = recommendationRepository;
        this.cooldownService = cooldownService;
        this.algorithm = algorithm;
    }

    public async Task<JobRecommendationResult?> GetNextCardAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default)
    {
        var ranked = await BuildRankedListAsync(userId, filters, cancellationToken).ConfigureAwait(false);
        if (ranked.Count == 0)
        {
            return null;
        }

        var (topRankedJob, score) = ranked[0];
        return await BuildCardWithShownRecordAsync(userId, topRankedJob, score, cancellationToken).ConfigureAwait(false);
    }

    public async Task<JobRecommendationResult?> RecalculateTopCardIgnoringCooldownAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default)
    {
        var ranked = await BuildRankedListIgnoringCooldownAsync(userId, filters, cancellationToken).ConfigureAwait(false);
        if (ranked.Count == 0)
        {
            return null;
        }

        var best = ranked[0];
        return await BuildCardWithShownRecordAsync(userId, best.Job, best.Score, cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<(Job Job, double Score)>> BuildRankedListIgnoringCooldownAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User not found.");

        var jobs = await GetFilteredJobsAsync(filters, user, cancellationToken).ConfigureAwait(false);
        var userSkills = await userSkillRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);

        var ranked = new List<(Job Job, double Score)>();
        foreach (var currentJob in jobs)
        {
            if (await matchService.GetByUserIdAndJobIdAsync(userId, currentJob.JobId, cancellationToken).ConfigureAwait(false) is not null)
            {
                continue;
            }

            var score = await ComputeCompatibilityScoreAsync(user, currentJob, userSkills, cancellationToken).ConfigureAwait(false);
            ranked.Add((currentJob, score));
        }

        ranked.Sort(CompareRankedJobsByScoreDescending);
        return ranked;
    }

    private async Task<double> ComputeCompatibilityScoreAsync(User user, Job job, IReadOnlyList<UserSkill> userSkills, CancellationToken cancellationToken)
    {
        var jobSkills = await jobSkillRepository.GetByJobIdAsync(job.JobId, cancellationToken).ConfigureAwait(false);
        return algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);
    }

    private async Task<List<(Job Job, double Score)>> BuildRankedListAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User not found.");

        var jobs = await GetFilteredJobsAsync(filters, user, cancellationToken).ConfigureAwait(false);
        var userSkills = await userSkillRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);

        var ranked = new List<(Job Job, double Score)>();
        foreach (var currentJob in jobs)
        {
            if (await matchService.GetByUserIdAndJobIdAsync(userId, currentJob.JobId, cancellationToken).ConfigureAwait(false) is not null)
            {
                continue;
            }

            if (await cooldownService.IsOnCooldownAsync(userId, currentJob.JobId, DateTime.UtcNow, cancellationToken).ConfigureAwait(false))
            {
                continue;
            }

            var score = await ComputeCompatibilityScoreAsync(user, currentJob, userSkills, cancellationToken).ConfigureAwait(false);
            ranked.Add((currentJob, score));
        }

        ranked.Sort(CompareRankedJobsByScoreDescending);
        return ranked;
    }

    private async Task<JobRecommendationResult> BuildCardWithShownRecordAsync(int userId, Job job, double score, CancellationToken cancellationToken)
    {
        var displayRecommendation = new Recommendation
        {
            UserId = userId,
            JobId = job.JobId,
            Timestamp = DateTime.UtcNow,
        };

        var saved = await recommendationRepository.AddAsync(displayRecommendation, cancellationToken).ConfigureAwait(false);
        return await CreateCardAsync(job, score, saved.RecommendationId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<JobRecommendationResult> CreateCardAsync(Job job, double score, int? displayRecommendationId, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(job.CompanyId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Company {job.CompanyId} not found.");

        var jobSkillRows = await jobSkillRepository.GetByJobIdAsync(job.JobId, cancellationToken).ConfigureAwait(false);
        var topSkills = JobRecommendationResult.TakeTopSkills(jobSkillRows);
        var allSkillLabels = new List<string>();
        foreach (var jobSkill in jobSkillRows)
        {
            var skillName = jobSkill.Skill?.Name ?? $"Skill #{jobSkill.SkillId}";
            allSkillLabels.Add($"{skillName} (min {jobSkill.RequiredLevel})");
        }

        return new JobRecommendationResult
        {
            Job = job,
            Company = company,
            CompatibilityScore = score,
            TopSkillLabels = topSkills,
            AllSkillLabels = allSkillLabels,
            DisplayRecommendationId = displayRecommendationId,
        };
    }

    public async Task<int> ApplyLikeAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default)
    {
        var targetJob = card.Job;
        if (await matchService.GetByUserIdAndJobIdAsync(userId, targetJob.JobId, cancellationToken).ConfigureAwait(false) is not null)
        {
            throw new InvalidOperationException("Already applied to this job.");
        }

        return await matchService.CreatePendingApplicationAsync(userId, targetJob.JobId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> ApplyDismissAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default)
    {
        var dismissedRecommendation = new Recommendation
        {
            UserId = userId,
            JobId = card.Job.JobId,
            Timestamp = DateTime.UtcNow,
        };

        var saved = await recommendationRepository.AddAsync(dismissedRecommendation, cancellationToken).ConfigureAwait(false);
        return saved.RecommendationId;
    }

    public async Task UndoLikeAsync(int matchId, int? displayRecommendationId, CancellationToken cancellationToken = default)
    {
        await matchService.RemoveApplicationAsync(matchId, cancellationToken).ConfigureAwait(false);
        if (displayRecommendationId is { } resolvedDisplayRecommendationId)
        {
            await recommendationRepository.RemoveAsync(resolvedDisplayRecommendationId, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task UndoDismissAsync(int dismissRecommendationId, int? displayRecommendationId, CancellationToken cancellationToken = default)
    {
        await recommendationRepository.RemoveAsync(dismissRecommendationId, cancellationToken).ConfigureAwait(false);
        if (displayRecommendationId is { } resolvedDisplayRecommendationId && resolvedDisplayRecommendationId != dismissRecommendationId)
        {
            await recommendationRepository.RemoveAsync(resolvedDisplayRecommendationId, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> PassesFiltersAsync(Job job, UserMatchmakingFilters filters, User user, CancellationToken cancellationToken)
    {
        if (filters.EmploymentTypes.Count > 0)
        {
            if (!filters.EmploymentTypes.Contains(job.EmploymentType))
            {
                return false;
            }
        }

        if (filters.ExperienceLevels.Count > 0)
        {
            var bucket = MapUserYearsToExperienceBucket(user.YearsOfExperience);
            if (!filters.ExperienceLevels.Contains(bucket))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.LocationSubstring))
        {
            if (job.Location.IndexOf(filters.LocationSubstring.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }
        }

        if (filters.SkillIds.Count > 0)
        {
            var jobSkillIds = await GetJobSkillIdSetAsync(job.JobId, cancellationToken).ConfigureAwait(false);
            if (!HasAnySkillIntersection(filters.SkillIds, jobSkillIds))
            {
                return false;
            }
        }

        return true;
    }

    public static string MapUserYearsToExperienceBucket(int yearsOfExperience)
    {
        const int InternshipThreshold = 2;
        const int EntryThreshold = 4;
        const int MidSeniorThreshold = 7;
        const int DirectorThreshold = 10;
        return yearsOfExperience switch
        {
            < InternshipThreshold => "Internship",
            < EntryThreshold => "Entry",
            < MidSeniorThreshold => "MidSenior",
            < DirectorThreshold => "Director",
            _ => "Executive",
        };
    }

    private async Task<List<Job>> GetFilteredJobsAsync(UserMatchmakingFilters filters, User user, CancellationToken cancellationToken)
    {
        var filteredJobs = new List<Job>();
        foreach (var job in await jobRepository.GetAllAsync(cancellationToken).ConfigureAwait(false))
        {
            if (await PassesFiltersAsync(job, filters, user, cancellationToken).ConfigureAwait(false))
            {
                filteredJobs.Add(job);
            }
        }

        return filteredJobs;
    }

    private async Task<HashSet<int>> GetJobSkillIdSetAsync(int jobId, CancellationToken cancellationToken)
    {
        var skillIds = new HashSet<int>();
        foreach (var jobSkill in await jobSkillRepository.GetByJobIdAsync(jobId, cancellationToken).ConfigureAwait(false))
        {
            skillIds.Add(jobSkill.SkillId);
        }

        return skillIds;
    }

    private static bool HasAnySkillIntersection(IReadOnlyCollection<int> filterSkillIds, HashSet<int> jobSkillIds)
    {
        foreach (var filterSkillId in filterSkillIds)
        {
            if (jobSkillIds.Contains(filterSkillId))
            {
                return true;
            }
        }

        return false;
    }

    private static int CompareRankedJobsByScoreDescending((Job Job, double Score) left, (Job Job, double Score) right)
    {
        return right.Score.CompareTo(left.Score);
    }
}
