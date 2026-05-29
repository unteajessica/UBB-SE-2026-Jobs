using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services.Interfaces;

namespace TestsAndInterviews.Tests.Helpers
{
    public class FakeJobsService : IJobsService
    {
        private List<JobPosting> jobs = new List<JobPosting>();
        private List<Skill> skills = new List<Skill>();

        public async Task<List<JobPosting>> GetAllJobsAsync()
        {
            return await Task.FromResult(jobs);
        }

        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            return await Task.FromResult(skills);
        }

        public async Task<int> AddJob(JobPosting jobPosting, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            jobPosting.CompanyId = companyId;
            jobs.Add(jobPosting);
            return await Task.FromResult(jobPosting.JobId);
        }

        public void AddJobDirectly(JobPosting job)
        {
            jobs.Add(job);
        }
    }
}
