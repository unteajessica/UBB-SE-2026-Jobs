namespace Tests_and_Interviews_API.Repositories.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;

    public interface IJobsRepository
    {
        IEnumerable<JobPosting> GetAllJobs();

        /// <summary>All skills in the catalog (for job creation checkboxes).</summary>
        IReadOnlyList<Skill> GetAllSkills();

        /// <summary>Skills linked to a job with required percentages.</summary>
        // IReadOnlyList<JobSkill> GetSkillsForJob(int jobId);

        /// <summary>Inserts a job (new job_id = MAX(job_id)+1) and optional job_skills rows. Returns the new id.</summary>
        int AddJob(JobPosting job, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks);

        /// <summary>
        /// Retrieves a single job posting by its unique identifier.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job posting.</param>
        /// <returns>The matching <see cref="JobPosting"/>, or <c>null</c> if not found.</returns>
        JobPosting? GetJobById(int jobId);

        /// <summary>
        /// Updates an existing job posting and replaces its associated skill links.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job posting to update.</param>
        /// <param name="updatedJob">The updated job posting data. Cannot be null.</param>
        /// <param name="skillLinks">The new list of skill links to associate with the job posting.</param>
        /// <returns><c>true</c> if the update succeeded; <c>false</c> if the job was not found.</returns>
        bool UpdateJob(int jobId, JobPosting updatedJob, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks);

        /// <summary>
        /// Deletes a job posting and all its associated skill links.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job posting to delete.</param>
        /// <returns><c>true</c> if the deletion succeeded; <c>false</c> if the job was not found.</returns>
        bool DeleteJob(int jobId);
    }
}
