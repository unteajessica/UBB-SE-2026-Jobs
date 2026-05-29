// <copyright file="GradingService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Services
{
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// GradingService acts as a proxy to the API grading endpoints.
    /// </summary>
    public class GradingService : IGradingService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="GradingService"/> class.
        /// </summary>
        public GradingService()
        {
            this.http = ApiClient.Http;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GradingService"/> class.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        public GradingService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        /// <summary>
        /// Grades single choice question.
        /// </summary>
        public async void GradeSingleChoice(Question question, Answer answer)
        {
            var request = new GradeRequest
            {
                Question = question,
                Answer = answer,
            };

            var response = await this.http.PostAsJsonAsync(
                "grading/single-choice",
                request);

            response.EnsureSuccessStatusCode();

            var gradedAnswer = await response.Content.ReadFromJsonAsync<Answer>();

            answer.Value = gradedAnswer.Value;
        }

        /// <summary>
        /// Grades multiple choice question.
        /// </summary>
        public async void GradeMultipleChoice(Question question, Answer answer)
        {
            var request = new GradeRequest
            {
                Question = question,
                Answer = answer,
            };

            var response = await this.http.PostAsJsonAsync(
                "grading/multiple-choice",
                request);

            response.EnsureSuccessStatusCode();

            var gradedAnswer = await response.Content.ReadFromJsonAsync<Answer>();

            answer.Value = gradedAnswer.Value;
        }

        /// <summary>
        /// Grades text question.
        /// </summary>
        public async void GradeText(Question question, Answer answer)
        {
            var request = new GradeRequest
            {
                Question = question,
                Answer = answer,
            };

            var response = await this.http.PostAsJsonAsync(
                "grading/text",
                request);

            response.EnsureSuccessStatusCode();

            var gradedAnswer = await response.Content.ReadFromJsonAsync<Answer>();

            answer.Value = gradedAnswer.Value;
        }

        /// <summary>
        /// Grades true/false question.
        /// </summary>
        public async void GradeTrueFalse(Question question, Answer answer)
        {
            var request = new GradeRequest
            {
                Question = question,
                Answer = answer,
            };

            var response = await this.http.PostAsJsonAsync(
                "grading/true-false",
                request);

            response.EnsureSuccessStatusCode();

            var gradedAnswer = await response.Content.ReadFromJsonAsync<Answer>();

            answer.Value = gradedAnswer.Value;
        }

        /// <summary>
        /// Calculates final score.
        /// </summary>
        public float CalculateFinalScore(TestAttempt attempt)
        {
            var response = this.http.PostAsJsonAsync(
                "grading/final-score",
                attempt).Result;

            response.EnsureSuccessStatusCode();

            var score = response.Content.ReadFromJsonAsync<float>().Result;

            attempt.Score = (decimal)score;

            return score;
        }

        /// <summary>
        /// DTO used for requests.
        /// </summary>
        private class GradeRequest
        {
            /// <summary>
            /// Gets or sets question.
            /// </summary>
            public Question Question { get; set; }

            /// <summary>
            /// Gets or sets answer.
            /// </summary>
            public Answer Answer { get; set; }
        }
    }
}