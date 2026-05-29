namespace Tests_and_Interviews_API.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;

    public class JobsRepository : IJobsRepository
    {
        private const int MinimumSkillPercentage = 1;
        private const int MaximumSkillPercentage = 100;

        private readonly AppDbContext appDbContext;

        public JobsRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        /// <inheritdoc />
        public IEnumerable<JobPosting> GetAllJobs()
        {
            return this.appDbContext.Jobs
                .Include(j => j.Company)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .ToList();
        }

        /// <inheritdoc />
        public IReadOnlyList<Skill> GetAllSkills()
        {
            return this.appDbContext.Skills.ToList();
        }

        /// <inheritdoc />
        public JobPosting? GetJobById(int jobId)
        {
            return this.appDbContext.Jobs
                .Include(j => j.Company)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefault(j => j.JobId == jobId);
        }

        /// <inheritdoc />
        public int AddJob(JobPosting jobPosting, int companyId, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            if (jobPosting == null)
            {
                throw new ArgumentNullException(nameof(jobPosting));
            }

            using var transaction = this.appDbContext.Database.BeginTransaction();

            try
            {
                jobPosting.CompanyId = companyId;
                jobPosting.AmountPayed ??= 0;
                jobPosting.PostedAt ??= DateTime.Now;

                this.appDbContext.Jobs.Add(jobPosting);
                this.appDbContext.SaveChanges();

                if (skillLinks != null)
                {
                    foreach (var (skillId, percentage) in skillLinks)
                    {
                        if (percentage < MinimumSkillPercentage || percentage > MaximumSkillPercentage)
                        {
                            continue;
                        }

                        this.appDbContext.JobSkills.Add(new JobSkill
                        {
                            JobId = jobPosting.JobId,
                            SkillId = skillId,
                            RequiredPercentage = percentage,
                        });
                    }

                    this.appDbContext.SaveChanges();
                }

                var company = this.appDbContext.Companies.Find(companyId);
                if (company != null)
                {
                    company.PostedJobsCount += 1;
                    this.appDbContext.SaveChanges();
                }

                transaction.Commit();
                return jobPosting.JobId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public bool UpdateJob(int jobId, JobPosting updatedJob, IReadOnlyList<(int SkillId, int RequiredPercentage)> skillLinks)
        {
            if (updatedJob == null)
            {
                throw new ArgumentNullException(nameof(updatedJob));
            }

            using var transaction = this.appDbContext.Database.BeginTransaction();

            try
            {
                JobPosting? existing = this.appDbContext.Jobs
                    .Include(j => j.JobSkills)
                    .FirstOrDefault(j => j.JobId == jobId);

                if (existing == null)
                {
                    return false;
                }

                // Update scalar fields only; CompanyId and PostedAt are preserved
                existing.JobTitle = updatedJob.JobTitle;
                existing.JobDescription = updatedJob.JobDescription;
                existing.AmountPayed = updatedJob.AmountPayed ?? existing.AmountPayed;

                // Replace skill links: remove old ones, insert new ones
                if (existing.JobSkills != null)
                {
                    this.appDbContext.JobSkills.RemoveRange(existing.JobSkills);
                }

                if (skillLinks != null)
                {
                    foreach (var (skillId, percentage) in skillLinks)
                    {
                        if (percentage < MinimumSkillPercentage || percentage > MaximumSkillPercentage)
                        {
                            continue;
                        }

                        this.appDbContext.JobSkills.Add(new JobSkill
                        {
                            JobId = jobId,
                            SkillId = skillId,
                            RequiredPercentage = percentage,
                        });
                    }
                }

                this.appDbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public bool DeleteJob(int jobId)
        {
            using var transaction = this.appDbContext.Database.BeginTransaction();

            try
            {
                JobPosting? job = this.appDbContext.Jobs
                    .Include(j => j.JobSkills)
                    .FirstOrDefault(j => j.JobId == jobId);

                if (job == null)
                {
                    return false;
                }

                // Remove skill links first to respect FK constraints
                if (job.JobSkills != null)
                {
                    this.appDbContext.JobSkills.RemoveRange(job.JobSkills);
                }

                this.appDbContext.Jobs.Remove(job);

                // Decrement the company's posted job count
                var company = this.appDbContext.Companies.Find(job.CompanyId);
                if (company != null && company.PostedJobsCount > 0)
                {
                    company.PostedJobsCount -= 1;
                }

                this.appDbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}