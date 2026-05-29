namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Linq;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;
    using Tests_and_Interviews_API.Validators;

    public class ProfileCompletionCalculator : IProfileCompletionCalculator
    {
        // Company has:
        //    this.Name = name;
        //    this.AboutUs = aboutus;
        //    this.Pfp_url = pfp_url;
        //    this.Logo_url = logo_url;
        //    this.Location = location;
        //    this.Email = email;
        private const int TotalRequiredTasksCount = 5;
        private const int MinimumRequiredPostedJobs = 5;
        private const int MinimumRequiredCollaborators = 2;
        private const int PercentageMultiplier = 100;
        private const int TopSkillsLimit = 3;
        private const int DaysToLookBack = -7;
        private const int EmptyCount = 0;

        private const string TaskUploadPictureText = "Upload company picture";
        private const string TaskAddDescriptionText = "Add company description";
        private const string TaskPostJobsText = "Post at least 5 jobs";
        private const string TaskAddCollaboratorsText = "Add 2 collaborators";
        private const string TaskCompleteMiniGameText = "Complete mini-game";

        private const string MessageNoApplicantsText = "No applicants yet. Start posting jobs!";
        private const string MessageGreatStartPrefix = "Great start! You have ";
        private const string MessageGreatStartSuffix = " new applicants.";
        private const string MessageFewerApplicantsPrefix = "You have ";
        private const string MessageFewerApplicantsSuffix = "% fewer applicants than last week.";
        private const string MessageMoreApplicantsPrefix = "Congrats! You have ";
        private const string MessageMoreApplicantsSuffix = "% more applicants than last week.";

        private readonly IJobsRepository jobsRepository;
        private readonly IApplicantRepository applicantRepository;

        public ProfileCompletionCalculator(IJobsRepository jobsRepository, IApplicantRepository applicantRepository)
        {
            this.jobsRepository = jobsRepository;
            this.applicantRepository = applicantRepository;
        }

        public (int percentage, List<string> remainingTasks) Calculate(Company company)
        {
            int completedTasksCount = EmptyCount;
            var remainingTasksList = new List<string>();

            if (!string.IsNullOrEmpty(company.ProfilePicturePath))
            {
                completedTasksCount++;
            }
            else
            {
                remainingTasksList.Add(TaskUploadPictureText);
            }

            if (!string.IsNullOrEmpty(company.AboutUs))
            {
                completedTasksCount++;
            }
            else
            {
                remainingTasksList.Add(TaskAddDescriptionText);
            }
            if (company.PostedJobsCount >= MinimumRequiredPostedJobs)
            {
                completedTasksCount++;
            }
            else
            {
                remainingTasksList.Add(TaskPostJobsText);
            }
            if (company.CollaboratorsCount >= MinimumRequiredCollaborators || company.CollaboratorsCount >= MinimumRequiredCollaborators)
            {
                completedTasksCount++;
            }
            else
            {
                remainingTasksList.Add(TaskAddCollaboratorsText);
            }
            if (IsMiniGameComplete(company.Game))
            {
                completedTasksCount++;
            }
            else
            {
                remainingTasksList.Add(TaskCompleteMiniGameText);
            }
            return ((completedTasksCount * PercentageMultiplier) / TotalRequiredTasksCount, remainingTasksList);
        }

        private static bool IsMiniGameComplete(Game game)
        {
            return game != null && game.IsPublished;
        }

        public (List<string> skillNames, List<int> percents) GetSkillsTop3(int companyId)
        {
            var companyJobsList = jobsRepository
                .GetAllJobs()
                .Where(job => job.Company != null && job.Company.CompanyId == companyId)
                .ToList();

            var skillCountsDictionary = new Dictionary<string, int>();
            int totalRequiredPercentage = EmptyCount;

            foreach (var job in companyJobsList)
            {
                if (job.JobSkills == null)
                {
                    continue;
                }
                foreach (var jobSkill in job.JobSkills)
                {
                    var skillName = jobSkill.Skill?.SkillName;
                    if (string.IsNullOrEmpty(skillName))
                    {
                        continue;
                    }
                    if (!skillCountsDictionary.ContainsKey(skillName))
                    {
                        skillCountsDictionary[skillName] = EmptyCount;
                    }
                    skillCountsDictionary[skillName] += jobSkill.RequiredPercentage;
                    totalRequiredPercentage += jobSkill.RequiredPercentage;
                }
            }

            var topSkillNamesList = new List<string>();
            var topSkillPercentagesList = new List<int>();

            if (totalRequiredPercentage == EmptyCount)
            {
                return (topSkillNamesList, topSkillPercentagesList);
            }
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

        public string ApplicantsMessage(int companyId)
        {
            var companyApplicantsList = applicantRepository.GetApplicantsByCompany(companyId);

            int currentWeekApplicantsCount = companyApplicantsList
                .Count(applicant => applicant.AppliedAt >= DateTime.Now.AddDays(DaysToLookBack));

            int previousWeekApplicantsCount = companyApplicantsList
                .Count(applicant => applicant.AppliedAt < DateTime.Now.AddDays(DaysToLookBack));

            if (previousWeekApplicantsCount == EmptyCount)
            {
                if (currentWeekApplicantsCount == EmptyCount)
                {
                    return MessageNoApplicantsText;
                }
                return $"{MessageGreatStartPrefix}{currentWeekApplicantsCount}{MessageGreatStartSuffix}";
            }

            double percentageChange = ((double)(currentWeekApplicantsCount - previousWeekApplicantsCount) / previousWeekApplicantsCount) * PercentageMultiplier;

            if (percentageChange < EmptyCount)
            {
                return $"{MessageFewerApplicantsPrefix}{Math.Abs((int)percentageChange)}{MessageFewerApplicantsSuffix}";
            }
            else
            {
                return $"{MessageMoreApplicantsPrefix}{(int)percentageChange}{MessageMoreApplicantsSuffix}";
            }
        }
    }
}