// <copyright file="InterviewSessionService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using Windows.Storage;
    using Windows.Storage.Streams;
    using static System.Collections.Specialized.BitVector32;

    /// <summary>
    /// Handles all business logic related to interview sessions.
    /// </summary>
    public class InterviewSessionService : IInterviewSessionService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewSessionService"/> class.
        /// </summary>
        public InterviewSessionService()
        {
            this.http = ApiClient.Http;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewSessionService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        public InterviewSessionService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        /// <summary>
        /// Loads the session and its questions, and marks the session as started.
        /// </summary>
        /// <param name="sessionId">The ID of the interview session.</param>
        /// <returns>A tuple containing the loaded interview session and its questions.</returns>
        public async Task<(InterviewSession? Session, List<Question> Questions)> StartSessionAsync(int sessionId)
        {
            InterviewSession? session = await this.GetSessionAsync(sessionId);
            if (session != null)
            {
                session.DateStart = DateTime.UtcNow;
                await this.UpdateSessionViaApiAsync(session);
            }
            HttpResponseMessage questionsResponse = await this.http.GetAsync($"questions/byposition/{session.PositionId}");
            questionsResponse.EnsureSuccessStatusCode();
            List<QuestionDto>? dtos = await questionsResponse.Content.ReadFromJsonAsync<List<QuestionDto>>();
            List<Question> questions = dtos?.Select(dto => dto.ToEntity()).ToList() ?? new List<Question>();
            return (session, questions);
        }

        /// <summary>
        /// Uploads a video recording file for the specified interview session and updates the session's video and
        /// status information.
        /// </summary>
        /// <remarks>After a successful upload, the server persists the video and status changes.</remarks>
        /// <param name="session">The interview session to which the recording will be attached. Must not be null.</param>
        /// <param name="recordingFilePath">The full file system path to the video recording file to upload. Must refer to an existing file.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SubmitRecordingAsync(InterviewSession session, string recordingFilePath)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(recordingFilePath);
            using IRandomAccessStreamWithContentType randomAccessStream = await file.OpenReadAsync();
            using Stream stream = randomAccessStream.AsStreamForRead();
            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", file.Name);
            HttpResponseMessage response = await this.http.PostAsync(
                $"interviewsessions/{session.Id}/video",
                content);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Saves the score and marks the session as completed.
        /// </summary>
        /// <param name="sessionId">The ID of the interview session.</param>
        /// <param name="score">The score given by the interviewer.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SubmitScoreAsync(int sessionId, float score)
        {
            InterviewSession? session = await this.GetSessionAsync(sessionId);
            if (session != null)
            {
                session.Score = (decimal)score;
                session.Status = InterviewStatus.Completed.ToString();
                await this.UpdateSessionViaApiAsync(session);
            }
        }

        /// <summary>
        /// Loads an interview session by its ID from the Web API.
        /// </summary>
        /// <param name="sessionId">The ID of the interview session.</param>
        /// <returns>The interview session corresponding to the specified ID.</returns>
        public async Task<InterviewSession> GetSessionAsync(int sessionId)
        {
            HttpResponseMessage response = await this.http.GetAsync($"interviewsessions/{sessionId}");
            response.EnsureSuccessStatusCode();
            InterviewSessionDto? dto = await response.Content.ReadFromJsonAsync<InterviewSessionDto>();
            return dto!.ToEntity();
        }

        /// <summary>
        /// Persists an updated interview session to the Web API.
        /// </summary>
        /// <param name="session">The session with updated values to send.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task UpdateSessionViaApiAsync(InterviewSession session)
        {
            HttpResponseMessage response = await this.http.PutAsJsonAsync(
                $"interviewsessions/{session.Id}",
                session.ToDto());
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<InterviewSession>> GetScheduledSessionsAsync()
        {
            HttpResponseMessage response = await this.http.GetAsync($"interviewsessions/scheduled");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<InterviewSession>();
            }

            response.EnsureSuccessStatusCode();
            List<InterviewSessionDto>? sessionsDto = await response.Content.ReadFromJsonAsync<List<InterviewSessionDto>>();
            return sessionsDto!.Select(session => session.ToEntity()).ToList();
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            HttpResponseMessage response = await this.http.DeleteAsync($"interviewsessions/{sessionId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<InterviewSession>> GetSessionsByStatusAsync(string status)
        {
            HttpResponseMessage response = await this.http.GetAsync($"interviewsessions/status/{status}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<InterviewSession>();
            }

            response.EnsureSuccessStatusCode();
            List<InterviewSessionDto>? sessionsDto = await response.Content.ReadFromJsonAsync<List<InterviewSessionDto>>();
            return sessionsDto!.Select(session => session.ToEntity()).ToList();
        }
    }
}