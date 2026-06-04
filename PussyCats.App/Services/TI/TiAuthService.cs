using System.Net.Http.Json;
using PussyCats.App.Dtos.TI;

namespace PussyCats.App.Services.TI;

public interface ITiAuthService
{
    /// <summary>
    /// Retrieves the list of companies from the T&amp;I API, used to populate the
    /// recruiter company picker. Returns an empty list if the call fails.
    /// </summary>
    Task<List<TiCompanyDto>> GetCompaniesAsync();

    /// <summary>
    /// Registers a user with the T&amp;I API. For recruiters this creates the
    /// Recruiter record tied to <paramref name="companyId"/>.
    /// </summary>
    Task<bool> RegisterAsync(string name, string email, string password, string role, int? companyId = null);
}

public class TiAuthService : ITiAuthService
{
    private readonly HttpClient http;

    public TiAuthService(HttpClient http) => this.http = http;

    public async Task<List<TiCompanyDto>> GetCompaniesAsync()
    {
        try
        {
            var companies = await http.GetFromJsonAsync<List<TiCompanyDto>>("api/companies");
            return companies ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> RegisterAsync(string name, string email, string password, string role, int? companyId = null)
    {
        var payload = new
        {
            Name = name,
            Email = email,
            Password = password,
            Role = role,
            CompanyId = companyId,
        };

        var response = await http.PostAsJsonAsync("api/auth/register", payload);
        return response.IsSuccessStatusCode;
    }
}
