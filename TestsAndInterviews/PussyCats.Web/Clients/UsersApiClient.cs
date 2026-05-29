using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using PussyCats.Web.Dtos;

namespace PussyCats.Web.Clients
{
    public class UsersApiClient
    {
        private const string ApiPath = "api/users";
        private readonly HttpClient http;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UsersApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            this.http = http;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserDto?> GetCurrentUser()
        {
            string? userIdValue = this.httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return null;
            }

            return await this.GetById(userId);
        }

        public async Task<UserDto?> GetById(int id)
        {
            HttpResponseMessage response = await this.http.GetAsync($"{ApiPath}/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
    }
}
