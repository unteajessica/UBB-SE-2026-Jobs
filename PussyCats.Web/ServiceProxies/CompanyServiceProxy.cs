using System.Diagnostics;
using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Helpers;
using PussyCats.Library.Services.CompanyService;

namespace PussyCats.Web.ServiceProxies;

public class CompanyServiceProxy : ICompanyService
{
    private readonly HttpClient http;

    public CompanyServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<IReadOnlyList<Company>>("api/company", JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? [];
    }

    public async Task<Company?> GetByIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/company/{companyId}", cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var company = await response.Content.ReadFromJsonAsync<Company>(JsonOptions.Default, cancellationToken).ConfigureAwait(false);
        return company;
    }

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/company", company, JsonOptions.Default, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Company>(JsonOptions.Default, cancellationToken).ConfigureAwait(false)
               ?? throw new InvalidOperationException("No company returned after creation.");
    }

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/company/{company.CompanyId}", company, JsonOptions.Default, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/company/{companyId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}