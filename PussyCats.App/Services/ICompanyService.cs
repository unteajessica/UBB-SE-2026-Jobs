using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface ICompanyService
{
    Task<Company?> GetByIdAsync(int companyId, CancellationToken ct = default);

    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default);

    Task<Company> AddAsync(Company company, CancellationToken ct = default);

    Task UpdateAsync(Company company, CancellationToken ct = default);

    Task RemoveAsync(int companyId, CancellationToken ct = default);
}
