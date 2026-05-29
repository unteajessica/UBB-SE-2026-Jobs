namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews.Models;

    public interface IJobsRepository
    {
        IEnumerable<JobPosting> GetAllJobs();

        /// <summary>All skills in the catalog (for job creation checkboxes).</summary>
        IReadOnlyList<Skill> GetAllSkills();

        /// <summary>Skills linked to a job with required percentages.</summary>
        // IReadOnlyList<JobSkill> GetSkillsForJob(int jobId);

        /// <summary>Inserts a job (new job_id = MAX(job_id)+1) and optional job_skills rows. Returns the new id.</summary>
        int AddJob(JobPosting job, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks);
    }
}
