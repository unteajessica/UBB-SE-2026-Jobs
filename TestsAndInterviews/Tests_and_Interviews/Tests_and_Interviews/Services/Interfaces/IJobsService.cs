using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface IJobsService
    {
        Task<List<JobPosting>> GetAllJobsAsync();
        Task<List<Skill>> GetAllSkillsAsync();
        Task<int> AddJob(JobPosting jobPosting, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks);

    }
}
