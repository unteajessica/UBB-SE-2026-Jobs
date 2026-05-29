using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Api;
using Tests_and_Interviews.Dtos;
using Tests_and_Interviews.Mappers;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services.Interfaces;

namespace Tests_and_Interviews.Services
{
    public class JobsService: IJobsService
    {
        private readonly HttpClient http;

        public JobsService()
        {
            this.http = ApiClient.Http;
        }

        public JobsService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        public async Task<List<JobPosting>> GetAllJobsAsync()
        {
            HttpResponseMessage response = await this.http.GetAsync($"jobs");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<JobPosting>();

            }

            response.EnsureSuccessStatusCode();
            List<JobPostingDto>? jobsDto = await response.Content.ReadFromJsonAsync<List<JobPostingDto>>();
            return jobsDto!.Select(job => job.ToEntity()).ToList();
        }

        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            HttpResponseMessage response = await this.http.GetAsync($"jobs/skills");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Skill>();
            }

            response.EnsureSuccessStatusCode();
            List<SkillDto>? skillsDto = await response.Content.ReadFromJsonAsync<List<SkillDto>>();
            return skillsDto!.Select(skill => skill.ToEntity()).ToList();
        }

        public async Task<int> AddJob(JobPosting jobPosting, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            if (jobPosting == null)
            {
                throw new ArgumentNullException(nameof(jobPosting));
            }

            AddJobDto content = new AddJobDto
            {
                JobPosting = jobPosting.ToDto(),
                CompanyId = companyId,
                SkillLinks = skillLinks.Select(link => new JobSkillDto
                {
                    SkillId = link.SkillId,
                    JobId = jobPosting.JobId,
                    RequiredPercentage = link.RequiredPercentage
                }).ToList()
            };

            HttpResponseMessage response = await this.http.PostAsJsonAsync($"jobs", content);
            response.EnsureSuccessStatusCode();
            int jobId = await response.Content.ReadFromJsonAsync<int>();
            return jobId;
        }
    }
}
