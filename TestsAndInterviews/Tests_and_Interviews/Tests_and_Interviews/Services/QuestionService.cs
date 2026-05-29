using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Api;
using Tests_and_Interviews.Dtos;
using Tests_and_Interviews.Mappers;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services.Interfaces;

namespace Tests_and_Interviews.Services
{
    public class QuestionService: IQuestionService
    {
        private readonly HttpClient http;

        public QuestionService()
        {
            this.http = ApiClient.Http;
        }

        public QuestionService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        public async Task<List<Question>> FindByTestIdAsync(int testId)
        {
            HttpResponseMessage response = await this.http.GetAsync($"questions/bytest/{testId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Question>();
            }

            response.EnsureSuccessStatusCode();
            List<QuestionDto>? questionsDto = await response.Content.ReadFromJsonAsync<List<QuestionDto>>();
            return questionsDto!.Select(question => question.ToEntity()).ToList();
        }
    }
}
