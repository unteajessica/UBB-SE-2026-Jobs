using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface ICompanyService
    {
        Task AddCompany(string companyName, string aboutUs, string pfpUrl, string logoUrl, string location, string email);
        Task<Company?> GetCompanyById(int companyId);
        Task UpdateCompany(Company company);
        Task RemoveCompany(int companyId);
        // PrintAll() omitted — was debug/console output only, no API equivalent
        Task<Company?> GetCompanyByName(string companyName);
    }
}