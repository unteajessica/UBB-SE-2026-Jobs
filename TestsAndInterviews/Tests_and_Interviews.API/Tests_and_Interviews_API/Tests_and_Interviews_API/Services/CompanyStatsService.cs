namespace Tests_and_Interviews_API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides company statistics operations such as skill aggregation and applicant trend messaging.
    /// </summary>
    public class CompanyStatsService : ICompanyStatsService
    {
        private const int PercentageMultiplier = 100;
        private const int TopSkillsLimit = 3;
        private const int DaysToLookBack = -7;
        private const int EmptyCount = 0;

        private const string MessageNoApplicantsText = "No applicants yet. Start posting jobs!";
        private const string MessageGreatStartPrefix = "Great start! You have ";
        private const string MessageGreatStartSuffix = " new applicants.";
        private const string MessageFewerApplicantsPrefix = "You have ";
        private const string MessageFewerApplicantsSuffix = "% fewer applicants than last week.";
        private const string MessageMoreApplicantsPrefix = "Congrats! You have ";
        private const string MessageMoreApplicantsSuffix = "% more applicants than last week.";

        private readonly IJobsRepository _jobsRepository;
        private readonly IApplicantRepository _applicantRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyStatsService"/> class.
        /// </summary>
        /// <param name="jobsRepository">The repository used to access job data. Cannot be null.</param>
        /// <param name="applicantRepository">The repository used to access applicant data. Cannot be null.</param>
        public CompanyStatsService(IJobsRepository jobsRepository, IApplicantRepository applicantRepository)
        {
            this._jobsRepository = jobsRepository;
            this._applicantRepository = applicantRepository;
        }

        /// <summary>
        /// Retrieves the top 3 skills required across all jobs posted by the specified company,
        /// along with their relative percentage of total required skill weight.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>A task containing a tuple of skill names and their corresponding percentages.</returns>
        public async Task<(List<string> skillNames, List<int> percents)> GetSkillsTop3Async(int companyId)
        {
            var allJobs = this._jobsRepository.GetAllJobs();
            var companyJobsList = allJobs
                .Where(job => job.CompanyId == companyId)
                .ToList();

            var skillCountsDictionary = new Dictionary<string, int>();
            int totalRequiredPercentage = EmptyCount;

            foreach (var job in companyJobsList)
            {
                if (job.JobSkills == null || job.JobSkills.Count == 0)
                    continue;

                foreach (var jobSkill in job.JobSkills)
                {
                    var skillName = jobSkill.Skill?.SkillName;
                    if (string.IsNullOrEmpty(skillName))
                        continue;

                    if (!skillCountsDictionary.ContainsKey(skillName))
                        skillCountsDictionary[skillName] = EmptyCount;

                    skillCountsDictionary[skillName] += jobSkill.RequiredPercentage;
                    totalRequiredPercentage += jobSkill.RequiredPercentage;
                }
            }

            var topSkillNamesList = new List<string>();
            var topSkillPercentagesList = new List<int>();

            if (totalRequiredPercentage == EmptyCount)
                return (topSkillNamesList, topSkillPercentagesList);

            var topThreeSkills = skillCountsDictionary
                .OrderByDescending(skillEntry => skillEntry.Value)
                .Take(TopSkillsLimit);

            foreach (var skillEntry in topThreeSkills)
            {
                topSkillNamesList.Add(skillEntry.Key);
                topSkillPercentagesList.Add((int)Math.Round((double)skillEntry.Value * PercentageMultiplier / totalRequiredPercentage));
            }

            return (topSkillNamesList, topSkillPercentagesList);
        }

        /// <summary>
        /// Generates a message describing applicant trends for the specified company
        /// by comparing the current week's applicants to the previous week's.
        /// </summary>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <returns>A task containing a human-readable applicant trend message.</returns>
        public async Task<string> ApplicantsMessageAsync(int companyId)
        {
            var companyApplicantsList = this._applicantRepository.GetApplicantsByCompany(companyId);

            int currentWeekApplicantsCount = companyApplicantsList
                .Count(applicant => applicant.AppliedAt >= DateTime.Now.AddDays(DaysToLookBack));

            int previousWeekApplicantsCount = companyApplicantsList
                .Count(applicant => applicant.AppliedAt < DateTime.Now.AddDays(DaysToLookBack));

            if (previousWeekApplicantsCount == EmptyCount)
            {
                if (currentWeekApplicantsCount == EmptyCount)
                    return MessageNoApplicantsText;

                return $"{MessageGreatStartPrefix}{currentWeekApplicantsCount}{MessageGreatStartSuffix}";
            }

            double percentageChange = ((double)(currentWeekApplicantsCount - previousWeekApplicantsCount) / previousWeekApplicantsCount) * PercentageMultiplier;

            if (percentageChange < EmptyCount)
                return $"{MessageFewerApplicantsPrefix}{Math.Abs((int)percentageChange)}{MessageFewerApplicantsSuffix}";

            return $"{MessageMoreApplicantsPrefix}{(int)percentageChange}{MessageMoreApplicantsSuffix}";
        }
    }
}