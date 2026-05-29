using PussyCats.Library.Domain;
using PussyCats.Library.Helpers;
using PussyCats.Library.Repositories.Companies;

namespace PussyCats.Library.Services.CompanyService;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task<Company?> GetByIdAsync(int companyId, CancellationToken cancellationToken = default)//TODO
    {
        var company= await companyRepository.GetByIdAsync(companyId, cancellationToken).ConfigureAwait(false);
        DebugToFile.Write("Service","company is "+company);
        return company;
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await companyRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        return await companyRepository.AddAsync(company, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        await companyRepository.UpdateAsync(company, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int companyId, CancellationToken cancellationToken = default)
    {
        await companyRepository.RemoveAsync(companyId, cancellationToken).ConfigureAwait(false);
    }
}
