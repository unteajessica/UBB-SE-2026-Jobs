using PussyCats.Web.Dtos;

namespace PussyCats.Web.Clients
{
    public class TestAttemptsApiClient
    {
        private readonly HttpClient _http;
        private const string s_apiPath = "/api/testattempts";

        public TestAttemptsApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<TestAttemptDto?> GetById(int id)
        {
            var response = await _http.GetAsync($"{s_apiPath}/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GET {s_apiPath}/{id} failed ({(int)response.StatusCode}): {body}");
            }

            return await response.Content.ReadFromJsonAsync<TestAttemptDto>();
        }

        public async Task<TestAttemptDto?> FindByUserAndTest(int userId, int testId)
        {
            var response = await _http.GetAsync($"{s_apiPath}/byuser/{userId}/bytest/{testId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GET {s_apiPath}/byuser/{userId}/bytest/{testId} failed ({(int)response.StatusCode}): {body}");
            }

            return await response.Content.ReadFromJsonAsync<TestAttemptDto>();
        }

        public async Task Create(TestAttemptDto dto)
        {
            var response = await _http.PostAsJsonAsync(s_apiPath, dto);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"POST {s_apiPath} failed ({(int)response.StatusCode}): {body}");
            }
        }
    }
}

