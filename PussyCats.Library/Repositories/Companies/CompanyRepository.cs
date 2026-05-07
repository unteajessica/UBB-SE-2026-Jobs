using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Companies;

public class CompanyRepository : ICompanyRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public CompanyRepository(PussyCatsDbContext database)
    {
        this.databaseContext = database;
    }

    /// <summary>
    /// Includes Jobs so company-detail screens can render the company's postings without a second
    /// round trip. Tracked because the typical caller intends to mutate.
    /// </summary>
    public async Task<Company?> GetByIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Companies
            .Include(company => company.Jobs)
            .FirstOrDefaultAsync(company => company.CompanyId == companyId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Companies
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        databaseContext.Companies.Add(company);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return company;
    }

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.Companies.Local.FirstOrDefault(existing => existing.CompanyId == company.CompanyId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(company);
        }
        else
        {
            databaseContext.Entry(company).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var company = await databaseContext.Companies.FindAsync(new object?[] { companyId }, cancellationToken).ConfigureAwait(false);
        if (company is null)
        {
            return;
        }
        databaseContext.Companies.Remove(company);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
