namespace Tests_and_Interviews_API.Repositories.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;

    public interface IApplicantRepository
    {
        Applicant GetApplicantById(int applicantId);
        public IEnumerable<Applicant> GetApplicantsByCompany(int companyId);
        IEnumerable<Applicant> GetApplicantsByJob(JobPosting job);
        void AddApplicant(Applicant applicant);
        void UpdateApplicant(Applicant applicant);
        void RemoveApplicant(int applicantId);
    }
}
