using PussyCats.Web.Dtos;

namespace PussyCats.Web.Clients
{
    public class AnswersApiClient
    {
        private readonly HttpClient _http;
        private static string s_apiPath = "/api/answers";

        public AnswersApiClient(HttpClient http)
        {
            this._http = http;
        }

        public async Task<List<AnswerDto>?> GetAnswersByAttempt(int attemptId)
        {
            var response = await this._http.GetAsync($"{s_apiPath}/byattempt/{attemptId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<AnswerDto>();

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GET {s_apiPath}/byattempt/{attemptId} failed ({(int)response.StatusCode}): {body}");
            }

            return await response.Content.ReadFromJsonAsync<List<AnswerDto>>();
        }

        public Task<List<AnswerDto>?> GetByAttempt(int attemptId) => GetAnswersByAttempt(attemptId);

        
        public async Task SaveAnswer(AnswerDto dto)
        {
            var response = await this._http.PostAsJsonAsync(s_apiPath, dto);
            response.EnsureSuccessStatusCode();
        }

      
        public Task Save(AnswerDto dto) => SaveAnswer(dto);
    }
}

