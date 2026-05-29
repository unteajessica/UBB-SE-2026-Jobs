namespace Tests_and_Interviews_API.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;

    public class ApplicantRepository : IApplicantRepository
    {
        private readonly AppDbContext appDbContext;

        public ApplicantRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        /// <inheritdoc/>
        public Applicant GetApplicantById(int applicantId)
        {
            return this.appDbContext.Applicants
                .Include(a => a.User)
                .Include(a => a.Job)
                .Include(a => a.RecommendedFromCompany)
                .FirstOrDefault(a => a.ApplicantId == applicantId);
        }

        /// <inheritdoc/>
        public IEnumerable<Applicant> GetApplicantsByCompany(int companyId)
        {
            return this.appDbContext.Applicants
                .Include(a => a.User)
                .Include(a => a.Job)
                .Include(a => a.RecommendedFromCompany)
                .Where(a => a.Job.CompanyId == companyId)
                .ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<Applicant> GetApplicantsByJob(JobPosting jobPosting)
        {
            if (jobPosting == null)
            {
                return new List<Applicant>();
            }

            return this.appDbContext.Applicants
                .Include(a => a.User)
                .Include(a => a.Job)
                .Include(a => a.RecommendedFromCompany)
                .Where(a => a.JobId == jobPosting.JobId)
                .ToList();
        }

        /// <inheritdoc/>
        public void AddApplicant(Applicant applicant)
        {
            this.appDbContext.Applicants.Add(applicant);
            this.appDbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void UpdateApplicant(Applicant applicant)
        {
            var existing = this.appDbContext.Applicants.Find(applicant.ApplicantId);
            if (existing == null)
            {
                return;
            }

            existing.AppTestGrade = applicant.AppTestGrade;
            existing.CvGrade = applicant.CvGrade;
            existing.CompanyTestGrade = applicant.CompanyTestGrade;
            existing.InterviewGrade = applicant.InterviewGrade;
            existing.ApplicationStatus = applicant.ApplicationStatus;

            this.appDbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void RemoveApplicant(int applicantId)
        {
            var applicant = this.appDbContext.Applicants.Find(applicantId);
            if (applicant != null)
            {
                this.appDbContext.Applicants.Remove(applicant);
                this.appDbContext.SaveChanges();
            }
        }
    }
}