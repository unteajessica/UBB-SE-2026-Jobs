namespace Tests_and_Interviews.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews.Data;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Repositories.Interfaces;

    public class JobsRepository : IJobsRepository
    {
        private const int MinimumSkillPercentage = 1;
        private const int MaximumSkillPercentage = 100;

        private readonly AppDbContext appDbContext;

        public JobsRepository()
        {
            this.appDbContext = new AppDbContext();
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
    }
}