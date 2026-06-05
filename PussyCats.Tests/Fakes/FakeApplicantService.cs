using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services.Interfaces;

namespace PussyCats.Tests.Fakes
{
    public class FakeApplicantService : IApplicantService
    {
        private List<Applicant> applicants = new List<Applicant>();

        public async Task<Applicant> GetApplicant(int applicantId)
        {
            return await Task.FromResult(applicants.FirstOrDefault(a => a.ApplicantId == applicantId));
        }

        public async Task<IEnumerable<Applicant>> GetApplicantsForJob(JobPosting job)
        {
            return await Task.FromResult(applicants.Where(a => a.JobId == job.JobId).ToList());
        }

        public async Task<IEnumerable<Applicant>> GetApplicantsByCompany(int companyId)
        {
            return await Task.FromResult(applicants.Where(a => a.Job?.CompanyId == companyId).ToList());
        }

        public async Task ProcessCv(int applicantId)
        {
            await Task.CompletedTask;
        }

        public async Task UpdateAppTestGrade(int applicantId, decimal grade)
        {
            var applicant = applicants.FirstOrDefault(a => a.ApplicantId == applicantId);
            if (applicant != null)
            {
                applicant.AppTestGrade = grade;
            }
            await Task.CompletedTask;
        }

        public async Task UpdateCompanyTestGrade(int applicantId, decimal grade)
        {
            var applicant = applicants.FirstOrDefault(a => a.ApplicantId == applicantId);
            if (applicant != null)
            {
                applicant.CompanyTestGrade = grade;
            }
            await Task.CompletedTask;
        }

        public async Task UpdateInterviewGrade(int applicantId, decimal grade)
        {
            var applicant = applicants.FirstOrDefault(a => a.ApplicantId == applicantId);
            if (applicant != null)
            {
                applicant.InterviewGrade = grade;
            }
            await Task.CompletedTask;
        }

        public async Task UpdateApplicant(Applicant applicant)
        {
            var existing = applicants.FirstOrDefault(a => a.ApplicantId == applicant.ApplicantId);
            if (existing != null)
            {
                applicants.Remove(existing);
            }
            applicants.Add(applicant);
            await Task.CompletedTask;
        }

        public async Task RemoveApplicant(int applicantId)
        {
            var applicant = applicants.FirstOrDefault(a => a.ApplicantId == applicantId);
            if (applicant != null)
            {
                applicants.Remove(applicant);
            }
            await Task.CompletedTask;
        }

        public async Task<decimal?> ScanCvXmlAsync(Applicant applicant)
        {
            return await Task.FromResult<decimal?>(null);
        }

        public void AddApplicantDirectly(Applicant applicant)
        {
            applicants.Add(applicant);
        }
    }
}
