using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Repositories.Interfaces;

namespace TestsAndInterviews.Tests.Helpers
{
    internal class FakeApplicantRepository : IApplicantRepository
    {
        private readonly List<Applicant> store = new List<Applicant>();

        public Applicant? LastAdded = null;
        public Applicant? LastUpdated = null;
        public int? LastRemovedId = null;

        public Applicant GetApplicantById(int applicantId)
        {
            return store.FirstOrDefault(a => a.ApplicantId == applicantId);
        }

        public IEnumerable<Applicant> GetApplicantsByCompany(int companyId)
        {
            return store.Where(a => a.Job?.Company != null && a.Job.Company.CompanyId == companyId).ToList();
        }

        public IEnumerable<Applicant> GetApplicantsByJob(JobPosting job)
        {
            if (job == null)
            {
                return new List<Applicant>();
            }

            return store.Where(a => a.Job != null && a.Job.JobId == job.JobId).ToList();
        }

        public void AddApplicant(Applicant applicant)
        {
            LastAdded = applicant;
            store.Add(applicant);
        }

        public void UpdateApplicant(Applicant applicant)
        {
            LastUpdated = applicant;
            var index = store.FindIndex(a => a.ApplicantId == applicant.ApplicantId);
            if (index >= 0)
            {
                store[index] = applicant;
            }
        }

        public void RemoveApplicant(int applicantId)
        {
            LastRemovedId = applicantId;
            store.RemoveAll(a => a.ApplicantId == applicantId);
        }
    }
}
