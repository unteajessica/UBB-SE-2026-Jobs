// <copyright file="ProfileCompletionCalculator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services.Interfaces;

    public class ProfileCompletionCalculator : IProfileCompletionCalculator
    {
        private const int TotalRequiredTasksCount = 5;
        private const int MinimumRequiredPostedJobs = 5;
        private const int MinimumRequiredCollaborators = 2;
        private const int PercentageMultiplier = 100;
        private const int EmptyCount = 0;

        private const string TaskUploadPictureText = "Upload company picture";
        private const string TaskAddDescriptionText = "Add company description";
        private const string TaskPostJobsText = "Post at least 5 jobs";
        private const string TaskAddCollaboratorsText = "Add 2 collaborators";
        private const string TaskCompleteMiniGameText = "Complete mini-game";

        private readonly HttpClient http;
        private readonly IJobsService? jobsService;
        private readonly IApplicantService? applicantService;

        public ProfileCompletionCalculator()
        {
            this.http = ApiClient.Http;
            this.jobsService = null;
            this.applicantService = null;
        }

        public ProfileCompletionCalculator(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
            this.jobsService = null;
            this.applicantService = null;
        }

        public ProfileCompletionCalculator(IJobsService jobsService, IApplicantService applicantService)
        {
            this.http = ApiClient.Http;
            this.jobsService = jobsService;
            this.applicantService = applicantService;
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

            if (company.CollaboratorsCount >= MinimumRequiredCollaborators)
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

        /// <inheritdoc />
        public async Task<(List<string> skillNames, List<int> percents)> GetSkillsTop3Async(int companyId)
        {
            if (this.jobsService != null)
            {
                var allJobs = await this.jobsService.GetAllJobsAsync();
                return ComputeTop3Skills(allJobs, companyId);
            }

            HttpResponseMessage response = await this.http.GetAsync($"companystats/{companyId}/skills/top3");

            if (!response.IsSuccessStatusCode)
            {
                return (new List<string>(), new List<int>());
            }

            var result = await response.Content.ReadFromJsonAsync<SkillsTop3Result>();
            return result != null ? (result.SkillNames, result.Percents) : (new List<string>(), new List<int>());
        }

        public (List<string> skillNames, List<int> percents) GetSkillsTop3(int companyId)
        {
            return this.GetSkillsTop3Async(companyId).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<string> ApplicantsMessage(int companyId)
        {
            if (this.applicantService != null)
            {
                return await this.ComputeApplicantsMessageAsync(companyId);
            }

            HttpResponseMessage response = await this.http.GetAsync($"companystats/{companyId}/applicantsmessage");

            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static (List<string> skillNames, List<int> percents) ComputeTop3Skills(
            IEnumerable<JobPosting> jobs, int companyId)
        {
            var skillAggregation = new Dictionary<string, int>();

            foreach (JobPosting job in jobs.Where(j => j.CompanyId == companyId))
            {
                if (job.JobSkills == null)
                {
                    continue;
                }

                foreach (JobSkill jobSkill in job.JobSkills)
                {
                    if (jobSkill.Skill?.SkillName == null)
                    {
                        continue;
                    }

                    string name = jobSkill.Skill.SkillName;
                    if (skillAggregation.TryGetValue(name, out int existing))
                    {
                        skillAggregation[name] = existing + jobSkill.RequiredPercentage;
                    }
                    else
                    {
                        skillAggregation[name] = jobSkill.RequiredPercentage;
                    }
                }
            }

            var top3 = skillAggregation
                .OrderByDescending(kv => kv.Value)
                .Take(3)
                .ToList();

            return (
                top3.Select(kv => kv.Key).ToList(),
                top3.Select(kv => kv.Value).ToList());
        }

        private async Task<string> ComputeApplicantsMessageAsync(int companyId)
        {
            var applicants = (await this.applicantService!.GetApplicantsByCompany(companyId)).ToList();

            if (!applicants.Any())
            {
                return "No applicants yet. Start posting jobs!";
            }

            var now = DateTime.Now;
            var currentWeekCutoff = now.AddDays(-7);
            var previousWeekCutoff = now.AddDays(-14);

            int currentWeekCount = applicants.Count(a => a.AppliedAt >= currentWeekCutoff);
            int previousWeekCount = applicants.Count(
                a => a.AppliedAt >= previousWeekCutoff && a.AppliedAt < currentWeekCutoff);

            if (previousWeekCount == 0)
            {
                return $"Great start! You have {currentWeekCount} new applicant(s) this week.";
            }

            if (currentWeekCount > previousWeekCount)
            {
                double changePercent = (double)(currentWeekCount - previousWeekCount) / previousWeekCount * 100;
                return $"Congrats! {changePercent:0}% more applicants this week.";
            }

            if (currentWeekCount < previousWeekCount)
            {
                double changePercent = (double)(previousWeekCount - currentWeekCount) / previousWeekCount * 100;
                return $"You have fewer applicants this week ({changePercent:0}% fewer).";
            }

            return $"You have {currentWeekCount} applicant(s) this week, same as last week.";
        }

        private static bool IsMiniGameComplete(Game game)
        {
            return game != null && game.IsPublished;
        }
    }
}
