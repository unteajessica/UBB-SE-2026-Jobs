// <copyright file="TestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// Calls the API for all test-related operations.
    /// </summary>
    public class TestService : ITestService
    {
        private readonly HttpClient http;
        private readonly IGradingService gradingService;
        private readonly ITimerService timerService;
        private readonly IAttemptValidationService validationService;
        private readonly IDataProcessingService dataProcessingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestService"/> class.
        /// </summary>
        public TestService()
        {
            this.http = ApiClient.Http;
            this.gradingService = null!;
            this.timerService = null!;
            this.validationService = null!;
            this.dataProcessingService = null!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestService"/> class.
        /// </summary>
        public TestService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
            this.gradingService = null!;
            this.timerService = null!;
            this.validationService = null!;
            this.dataProcessingService = null!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestService"/> class with injected services.
        /// </summary>
        public TestService(
            IGradingService gradingService,
            ITimerService timerService,
            IAttemptValidationService validationService,
            IDataProcessingService dataProcessingService)
        {
            this.http = ApiClient.Http;
            this.gradingService = gradingService;
            this.timerService = timerService;
            this.validationService = validationService;
            this.dataProcessingService = dataProcessingService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestService"/> class with injected services and a custom HTTP client.
        /// </summary>
        public TestService(
            IGradingService gradingService,
            ITimerService timerService,
            IAttemptValidationService validationService,
            IDataProcessingService dataProcessingService,
            HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
            this.gradingService = gradingService;
            this.timerService = timerService;
            this.validationService = validationService;
            this.dataProcessingService = dataProcessingService;
        }

        /// <inheritdoc/>
        public async Task StartTestAsync(int userId, int testId)
        {
            var attempt = new TestAttempt { ExternalUserId = userId, TestId = testId };
            attempt.Start();

            HttpResponseMessage response = await this.http.PostAsJsonAsync("tests/start", attempt.ToDto());
            response.EnsureSuccessStatusCode();

            TestAttemptDto? createdDto = await response.Content.ReadFromJsonAsync<TestAttemptDto>();
            if (createdDto != null && this.timerService != null)
            {
                this.timerService.StartTimer(createdDto.Id);
            }
        }

        /// <inheritdoc/>
        public async Task SubmitTestAsync(int attemptId)
        {
            HttpResponseMessage attemptResponse = await this.http.GetAsync($"testattempts/{attemptId}");
            attemptResponse.EnsureSuccessStatusCode();
            TestAttemptDto? attemptDto = await attemptResponse.Content.ReadFromJsonAsync<TestAttemptDto>();
            TestAttempt attempt = attemptDto!.ToEntity();

            HttpResponseMessage answersResponse = await this.http.GetAsync($"answers/byattempt/{attemptId}");
            answersResponse.EnsureSuccessStatusCode();
            List<AnswerDto>? answerDtos = await answersResponse.Content.ReadFromJsonAsync<List<AnswerDto>>();

            if (answerDtos != null && this.gradingService != null)
            {
                foreach (AnswerDto answerDto in answerDtos)
                {
                    Answer answer = answerDto.ToEntity();
                    if (answer.Question == null)
                    {
                        continue;
                    }

                    switch (answer.Question.QuestionTypeString)
                    {
                        case nameof(QuestionType.SINGLE_CHOICE):
                            this.gradingService.GradeSingleChoice(answer.Question, answer);
                            break;
                        case nameof(QuestionType.MULTIPLE_CHOICE):
                            this.gradingService.GradeMultipleChoice(answer.Question, answer);
                            break;
                        case nameof(QuestionType.TEXT):
                            this.gradingService.GradeText(answer.Question, answer);
                            break;
                        case nameof(QuestionType.TRUE_FALSE):
                            this.gradingService.GradeTrueFalse(answer.Question, answer);
                            break;
                    }
                }
            }

            attempt.Submit();
            HttpResponseMessage putResponse = await this.http.PutAsJsonAsync($"testattempts/{attemptId}", attempt.ToDto());
            putResponse.EnsureSuccessStatusCode();
        }

        /// <inheritdoc/>
        public async Task<float> SubmitAttemptAsync(int userId, int testId, IEnumerable<AnswerDto> answers)
        {
            HttpResponseMessage getResponse = await this.http.GetAsync($"testattempts/byuser/{userId}/bytest/{testId}");
            getResponse.EnsureSuccessStatusCode();
            TestAttemptDto? existingDto = await getResponse.Content.ReadFromJsonAsync<TestAttemptDto>();
            int attemptId = existingDto!.Id;

            await this.SubmitTestAsync(attemptId);

            if (this.dataProcessingService != null)
            {
                await this.dataProcessingService.ProcessFinalizedAttemptAsync(attemptId);
            }

            HttpResponseMessage finalResponse = await this.http.GetAsync($"testattempts/byuser/{userId}/bytest/{testId}");
            finalResponse.EnsureSuccessStatusCode();
            TestAttemptDto? finalDto = await finalResponse.Content.ReadFromJsonAsync<TestAttemptDto>();
            return (float)(finalDto!.Score ?? 0m);
        }

        /// <inheritdoc/>
        public async Task<Test?> GetNextAvailableTestAsync(string category)
        {
            HttpResponseMessage response = await this.http.GetAsync($"tests/bycategory/{category}");
            response.EnsureSuccessStatusCode();

            List<TestDto>? dtos = await response.Content.ReadFromJsonAsync<List<TestDto>>();

            if (dtos == null || dtos.Count == 0)
            {
                return null;
            }

            return dtos[0].ToEntity();
        }

        /// <inheritdoc/>
        public async Task<List<Test>> FindTestsByCategoryAsync(string category)
        {
            HttpResponseMessage response = await this.http.GetAsync($"tests/bycategory/{category}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Test>();
            }

            response.EnsureSuccessStatusCode();
            List<TestDto>? testsDto = await response.Content.ReadFromJsonAsync<List<TestDto>>();
            return testsDto!.Select(t => t.ToEntity()).ToList();
        }

        /// <inheritdoc/>
        public async Task<Test> FindByIdAsync(int id)
        {
            HttpResponseMessage response = await this.http.GetAsync($"tests/{id}");
            response.EnsureSuccessStatusCode();
            TestDto? testDto = await response.Content.ReadFromJsonAsync<TestDto>();
            return testDto!.ToEntity();
        }
    }
}
