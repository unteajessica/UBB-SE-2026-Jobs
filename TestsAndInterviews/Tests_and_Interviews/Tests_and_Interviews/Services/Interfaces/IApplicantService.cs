using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface IApplicantService
    {
        Task<IEnumerable<Applicant>> GetApplicantsForJob(JobPosting job);
        Task<Applicant> GetApplicant(int applicantId);
        Task<IEnumerable<Applicant>> GetApplicantsByCompany(int companyId);
        Task ProcessCv(int applicantId);
        Task UpdateAppTestGrade(int applicantId, decimal grade);
        Task UpdateCompanyTestGrade(int applicantId, decimal grade);
        Task UpdateInterviewGrade(int applicantId, decimal grade);
        Task UpdateApplicant(Applicant applicant);
        Task RemoveApplicant(int applicantId);
        Task<decimal?> ScanCvXmlAsync(Applicant applicant);
    }
}