namespace Tests_and_Interviews_API.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;

    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext appDbContext;

        public PaymentRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        /// <inheritdoc />
        public void UpdateJobPayment(int jobId, int paymentAmount)
        {
            var job = this.appDbContext.Jobs.Find(jobId);
            if (job == null)
            {
                throw new System.Exception("Job ID not found. Payment not applied to database.");
            }

            job.AmountPayed = paymentAmount;
            this.appDbContext.SaveChanges();
        }

        /// <inheritdoc />
        public List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel)
        {
            return this.appDbContext.Jobs
                .Where(j => j.JobType == jobType && j.ExperienceLevel == experienceLevel)
                .Join(
                    this.appDbContext.Companies,
                    j => j.CompanyId,
                    c => c.CompanyId,
                    (j, c) => new JobPaymentInfo
                    {
                        CompanyName = c.Name,
                        JobTitle = j.JobTitle,
                        AmountPayed = j.AmountPayed ?? 0,
                    })
                .ToList();
        }

        /// <inheritdoc />
        public List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount)
        {
            var currentJob = this.appDbContext.Jobs.Find(currentJobId);
            if (currentJob == null)
            {
                return [];
            }

            return this.appDbContext.Jobs
                .Where(j => j.JobId != currentJobId
                    && j.JobType == currentJob.JobType
                    && j.ExperienceLevel == currentJob.ExperienceLevel
                    && (j.AmountPayed == null || j.AmountPayed < newPaymentAmount))
                .Join(
                    this.appDbContext.Companies.Where(c => c.Email != null && c.Email != string.Empty),
                    j => j.CompanyId,
                    c => c.CompanyId,
                    (j, c) => c.Email)
                .Distinct()
                .ToList();
        }
    }
}