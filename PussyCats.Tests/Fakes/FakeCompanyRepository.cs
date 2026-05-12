using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Companies;

namespace PussyCats.Tests.Fakes;

public class FakeCompanyRepository : ICompanyRepository
{
    private readonly Dictionary<int, Company> companiesById = new();

    public void Seed(params Company[] companies)
    {
        foreach (var company in companies)
        {
            companiesById[company.CompanyId] = company;
        }
    }

    public Task<Company?> GetByIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        companiesById.TryGetValue(companyId, out var company);
        return Task.FromResult(company);
    }

    public Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Company> snapshot = companiesById.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        if (company.CompanyId == 0)
        {
            company.CompanyId = NextId();
        }
        companiesById[company.CompanyId] = company;
        return Task.FromResult(company);
    }

    public Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        companiesById[company.CompanyId] = company;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int companyId, CancellationToken cancellationToken = default)
    {
        companiesById.Remove(companyId);
        return Task.CompletedTask;
    }

    private int NextId() => companiesById.Count == 0 ? 1 : companiesById.Keys.Max() + 1;
}
