namespace Tests_and_Interviews.Web.Clients
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Web.Dtos;

    /// <summary>
    /// Client service responsible for communicating with the backend test API.
    /// </summary>
    public class TestsApiClient
    {
        private readonly HttpClient http;
        private static readonly string ApiPath = "api/tests";
        private static readonly string AttemptsApiPath = "api/testattempts";
        private static readonly string QuestionsApiPath = "api/questions";

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsApiClient"/> class.
        /// </summary>
        /// <param name="http">The HTTP client.</param>
        public TestsApiClient(HttpClient http)
        {
            this.http = http;
        }

        public async Task<List<TestDto>> GetAll()
        {
            return await this.http.GetFromJsonAsync<List<TestDto>>(ApiPath);
        }

        public async Task<List<string>> GetCategories()
        {
            return await this.http.GetFromJsonAsync<List<string>>($"{ApiPath}/categories");
        }

        public async Task<List<TestDto>> GetByCategory(string category)
        {
            return await this.http.GetFromJsonAsync<List<TestDto>>($"{ApiPath}/bycategory/{category}");
        }

        public async Task<TestDto?> GetById(int id)
        {
            return await this.http.GetFromJsonAsync<TestDto>($"{ApiPath}/{id}");
        }

        public async Task<TestDto?> Create(TestDto dto)
        {
            var response = await this.http.PostAsJsonAsync(ApiPath, dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TestDto>();
        }

        public async Task Update(int id, TestDto dto)
        {
            var response = await this.http.PutAsJsonAsync($"{ApiPath}/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task Delete(int id)
        {
            var response = await this.http.DeleteAsync($"{ApiPath}/{id}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Packages the candidate's answers and submits them to the API for server-side grading.
        /// </summary>
        public async Task<float> SubmitAttemptAsync(int userId, int testId, IEnumerable<AnswerDto> answers)
        {
            var payload = new
            {
                UserId = userId,
                TestId = testId,
                Answers = answers
            };

            var response = await this.http.PostAsJsonAsync($"{AttemptsApiPath}/submit", payload);

            if (!response.IsSuccessStatusCode)
            {
                return 0f;
            }

            return await response.Content.ReadFromJsonAsync<float>();
        }

        /// <summary>
        /// Asynchronously retrieves all questions associated with a specific test.
        /// </summary>
        /// <param name="testId">The ID of the test.</param>
        /// <returns>A list of QuestionDto objects.</returns>
        public async Task<List<QuestionDto>> GetQuestionsByTestIdAsync(int testId)
        {
            var response = await this.http.GetAsync($"{QuestionsApiPath}/bytest/{testId}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<QuestionDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<QuestionDto>>() ?? new List<QuestionDto>();
        }

        /// <summary>
        /// Asynchronously checks if an active attempt already exists for the user and test.
        /// </summary>
        public async Task<bool> AttemptExistsAsync(int userId, int testId)
        {
            var response = await this.http.GetAsync($"{AttemptsApiPath}/byuser/{userId}/bytest/{testId}");

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Asynchronously creates a new test attempt in the database with an IN_PROGRESS status.
        /// </summary>
        public async Task StartAttemptAsync(int userId, int testId)
        {
            var payload = new
            {
                UserId = userId,
                ExternalUserId = userId,
                TestId = testId,
                Status = "IN_PROGRESS",
                StartedAt = DateTime.UtcNow
            };

            await this.http.PostAsJsonAsync(AttemptsApiPath, payload);
        }

        /// <summary>
        /// Asynchronously retrieves the active attempt for a specific user and test.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="testId">The ID of the test.</param>
        /// <returns>A single test attempt, or null if none exists.</returns>
        public async Task<TestAttemptDto?> GetAttemptByUserAndTestAsync(int userId, int testId)
        {
            var response = await this.http.GetAsync($"{AttemptsApiPath}/byuser/{userId}/bytest/{testId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TestAttemptDto>();
        }

        /// <summary>
        /// Updates an existing test attempt via the API. This is used to submit the final answers and trigger grading.
        /// </summary>
        /// <param name="attemptId">The ID of the attempt being updated.</param>
        /// <param name="attemptDto">The attempt object containing the candidate's final answers.</param>
        /// <returns>The graded test attempt returned by the API, or null if the request fails.</returns>
        public async Task<TestAttemptDto?> UpdateAttemptAsync(int attemptId, TestAttemptDto attemptDto)
        {
            var response = await this.http.PutAsJsonAsync($"{AttemptsApiPath}/{attemptId}", attemptDto);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TestAttemptDto>();
        }

        /// <summary>
        /// Sends a question and answer to the API to be graded.
        /// </summary>
        public async Task<AnswerDto> GradeAnswerAsync(string questionType, object gradeRequestPayload)
        {
            string endpoint = questionType switch
            {
                "SINGLE_CHOICE" => "single-choice",
                "MULTIPLE_CHOICE" => "multiple-choice",
                "TRUE_FALSE" => "true-false",
                "TEXT" => "text",
                _ => "text"
            };

            var response = await this.http.PostAsJsonAsync($"api/grading/{endpoint}", gradeRequestPayload);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<AnswerDto>() ?? new AnswerDto();
        }

        /// <summary>
        /// Saves a graded answer to the database via the API.
        /// </summary>
        public async Task SaveAnswerAsync(AnswerDto dto)
        {
            var response = await this.http.PostAsJsonAsync("api/answers", dto);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Sends a mock TestAttempt payload containing the graded answers to the API to calculate the final score.
        /// </summary>
        public async Task<float> CalculateFinalScoreAsync(object attemptPayload)
        {
            var response = await this.http.PostAsJsonAsync("api/grading/final-score", attemptPayload);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<float>();
        }

        /// <summary>
        /// Retrieves all valid attempts for a specific test to check completion history.
        /// </summary>
        public async Task<List<TestAttemptDto>> GetValidAttemptsByTestIdAsync(int testId)
        {
            var response = await this.http.GetAsync($"{AttemptsApiPath}/valid/bytest/{testId}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<TestAttemptDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<TestAttemptDto>>() ?? new List<TestAttemptDto>();
        }

        /// <summary>
        /// Retrieves all answers submitted for a specific attempt to verify completion.
        /// </summary>
        public async Task<List<AnswerDto>> GetAnswersByAttemptIdAsync(int attemptId)
        {
            var response = await this.http.GetAsync($"api/answers/byattempt/{attemptId}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<AnswerDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<AnswerDto>>() ?? new List<AnswerDto>();
        }
    }
}