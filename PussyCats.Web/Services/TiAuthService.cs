namespace PussyCats.Web.Services
{
    using System.Net.Http.Json;
    using PussyCats.Web.Dtos;
    using PussyCats.Web.Models;

    /// <summary>
    /// Calls the API authentication endpoints.
    /// </summary>
    public class TiAuthService : ITiAuthService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="TiAuthService"/> class.
        /// </summary>
        /// <param name="http">The HTTP client used to call the API.</param>
        public TiAuthService(HttpClient http)
        {
            this.http = http;
        }

        /// <inheritdoc/>
        public async Task<AuthResponseModel?> LoginAsync(string email, string password)
        {
            var payload = new { Email = email, Password = password };
            HttpResponseMessage response =
                await this.http.PostAsJsonAsync("api/auth/login", payload);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AuthResponseModel>();
        }

        /// <inheritdoc/>
        public async Task<AuthResponseModel?> RegisterAsync(
            string name, string email, string password, string role, int? companyId = null)
        {
            var payload = new
            {
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                CompanyId = companyId,
            };

            HttpResponseMessage response =
                await this.http.PostAsJsonAsync("api/auth/register", payload);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AuthResponseModel>();
        }

        /// <inheritdoc/>
        public async Task<List<CompanyDto>> GetCompaniesAsync()
        {
            try
            {
                var companies = await this.http.GetFromJsonAsync<List<CompanyDto>>("api/companies");
                return companies ?? new List<CompanyDto>();
            }
            catch
            {
                return new List<CompanyDto>();
            }
        }
    }
}


