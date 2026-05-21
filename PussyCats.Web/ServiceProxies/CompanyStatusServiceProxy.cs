using System.Net.Http.Json;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.CompanyStatusService;

namespace PussyCats.Web.ServiceProxies;

public class CompanyStatusServiceProxy : ICompanyStatusService
{
    private readonly HttpClient http;

    public CompanyStatusServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<List<UserApplicationResult>>(
                   $"api/company-status/companies/{companyId}/applicants",
                   cancellationToken)
               ?? new List<UserApplicationResult>();
    }

    public async Task<UserApplicationResult?> GetApplicantByMatchIdAsync(
        int companyId,
        int matchId,
        CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync(
            $"api/company-status/companies/{companyId}/applicants/{matchId}",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserApplicationResult>(cancellationToken: cancellationToken);
    }
}
