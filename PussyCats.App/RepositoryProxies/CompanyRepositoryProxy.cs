using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Companies;

namespace PussyCats.App.RepositoryProxies;

public class CompanyRepositoryProxy : ICompanyRepository
{
    private readonly HttpClient http;

    public CompanyRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Company?> GetByIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Company>(http, $"api/companies/{companyId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Company>(http, "api/companies", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/companies", company, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Company>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/companies/{company.CompanyId}", company, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int companyId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/companies/{companyId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
