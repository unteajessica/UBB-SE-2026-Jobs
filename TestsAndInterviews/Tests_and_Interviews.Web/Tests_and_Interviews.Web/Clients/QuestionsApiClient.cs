using Tests_and_Interviews.Web.Dtos;

namespace Tests_and_Interviews.Web.Clients
{
    public class QuestionsApiClient
    {
        private readonly HttpClient _http;
        private static string s_apiPath = "/api/questions";

        public QuestionsApiClient(HttpClient http)
        {
            this._http = http;
        }

        public async Task<List<QuestionDto>> GetQuestions(int? testId = null)
        {
            if (testId.HasValue)
            {
                return await this._http.GetFromJsonAsync<List<QuestionDto>>($"{s_apiPath}/bytest/{testId.Value}");
            }

            return await this._http.GetFromJsonAsync<List<QuestionDto>>(s_apiPath);
        }

        public async Task<QuestionDto?> GetQuestion(int id)
        {
            return await this._http.GetFromJsonAsync<QuestionDto>($"{s_apiPath}/{id}");
        }

        public async Task<List<QuestionDto>?> GetByTest(int testId)
        {
            var response = await this._http.GetAsync($"{s_apiPath}/bytest/{testId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<QuestionDto>();

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<QuestionDto>>();
        }

       

        public async Task<QuestionDto?> Create(QuestionDto dto)
        {
            var response = await this._http.PostAsJsonAsync(s_apiPath, dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<QuestionDto>();
        }

        public async Task Update(int id, QuestionDto dto)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{s_apiPath}/{id}")
            {
                Content = JsonContent.Create(dto)
            };

            var response = await this._http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string body = string.Empty;
                try { body = await response.Content.ReadAsStringAsync(); } catch { }
                throw new HttpRequestException($"Request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
            }
        }

        public async Task Delete(int id)
        {
            var response = await this._http.DeleteAsync($"{s_apiPath}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                string body = string.Empty;
                try { body = await response.Content.ReadAsStringAsync(); } catch { }
                throw new HttpRequestException($"Request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {body}");
            }
        }

    }
}
