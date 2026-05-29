using System.Net;
using System.Net.Http.Json;
using Tests_and_Interviews.Web.Dtos;

namespace Tests_and_Interviews.Web.Clients
{
    public class LeaderboardApiClient
    {
        private const string ApiPath = "api/leaderboard";
        private readonly HttpClient http;

        public LeaderboardApiClient(HttpClient http)
        {
            this.http = http;
        }

        public async Task RecalculateLeaderboardAsync(int testId)
        {
            HttpResponseMessage response = await this.http.PostAsync($"{ApiPath}/recalculate/{testId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<LeaderboardEntryDto>> GetByTestId(int testId)
        {
            HttpResponseMessage response = await this.http.GetAsync($"{ApiPath}/bytest/{testId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<LeaderboardEntryDto>();
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>() ?? new List<LeaderboardEntryDto>();
        }

        public async Task<List<LeaderboardEntryDto>> GetTopByTestId(int testId, int limit)
        {
            HttpResponseMessage response = await this.http.GetAsync($"{ApiPath}/bytest/{testId}/top/{limit}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<LeaderboardEntryDto>();
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>() ?? new List<LeaderboardEntryDto>();
        }

        public async Task<LeaderboardEntryDto?> GetUserEntry(int testId, int userId)
        {
            HttpResponseMessage response = await this.http.GetAsync($"{ApiPath}/bytest/{testId}/byuser/{userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LeaderboardEntryDto>();
        }
    }
}