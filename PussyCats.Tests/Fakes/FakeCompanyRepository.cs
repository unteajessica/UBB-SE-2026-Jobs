using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Companies;

namespace PussyCats.Tests.Fakes;

public class FakeCompanyRepository : ICompanyRepository
{
    private readonly Dictionary<int, Company> store = new();

    public void Seed(params Company[] companies)
    {
        foreach (var company in companies)
        {
            store[company.CompanyId] = company;
        }
    }

    public Task<Company?> GetByIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue(companyId, out var company);
        return Task.FromResult(company);
    }

    public Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Company> snapshot = store.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        if (company.CompanyId == 0)
        {
            company.CompanyId = NextId();
        }
        store[company.CompanyId] = company;
        return Task.FromResult(company);
    }

    public Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        store[company.CompanyId] = company;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int companyId, CancellationToken cancellationToken = default)
    {
        store.Remove(companyId);
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
