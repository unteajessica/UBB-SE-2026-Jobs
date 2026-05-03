using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Companies;

namespace PussyCats.App.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task<Company?> GetByIdAsync(int companyId, CancellationToken ct = default)
    {
        return await companyRepository.GetByIdAsync(companyId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default)
    {
        return await companyRepository.GetAllAsync(ct).ConfigureAwait(false);
    }

    public async Task<Company> AddAsync(Company company, CancellationToken ct = default)
    {
        return await companyRepository.AddAsync(company, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Company company, CancellationToken ct = default)
    {
        await companyRepository.UpdateAsync(company, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int companyId, CancellationToken ct = default)
    {
        await companyRepository.RemoveAsync(companyId, ct).ConfigureAwait(false);
    }
}
