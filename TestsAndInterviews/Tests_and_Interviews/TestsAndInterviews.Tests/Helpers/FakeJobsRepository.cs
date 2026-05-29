using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Repositories.Interfaces;

namespace TestsAndInterviews.Tests.Helpers
{
    internal class FakeJobsRepository : IJobsRepository
    {
        public List<JobPosting> Jobs = new List<JobPosting>();
        public List<Skill> Skills = new List<Skill>();

        public IEnumerable<JobPosting> GetAllJobs()
        {
            return Jobs;
        }

        public IReadOnlyList<Skill> GetAllSkills()
        {
            return Skills;
        }

        public int AddJob(JobPosting job, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            Jobs.Add(job);
            return job.JobId;
        }
    }
}