// <copyright file="InterviewSessionsApiClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Web.Clients
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using PussyCats.Web.Dtos;

    /// <summary>
    /// HTTP client for the Interview Sessions and Bookings API endpoints, used by the MVC web app.
    /// </summary>
    public class InterviewSessionsApiClient
    {
        private readonly HttpClient http;
        private const string SessionsPath = "api/interviewsessions";
        private const string BookingsPath = "api/bookings";

        public InterviewSessionsApiClient(HttpClient http)
        {
            this.http = http;
        }

        /// <summary>
        /// Retrieves all sessions currently in a scheduled state.
        /// </summary>
        public async Task<List<InterviewSessionDto>> GetScheduledAsync()
        {
            return await this.http.GetFromJsonAsync<List<InterviewSessionDto>>(
                $"{SessionsPath}/scheduled") ?? new List<InterviewSessionDto>();
        }

        /// <summary>
        /// Retrieves all sessions with the given status.
        /// </summary>
        public async Task<List<InterviewSessionDto>> GetByStatusAsync(string status)
        {
            return await this.http.GetFromJsonAsync<List<InterviewSessionDto>>(
                $"{SessionsPath}/status/{status}") ?? new List<InterviewSessionDto>();
        }

        /// <summary>
        /// Confirms a booking for a candidate by reserving the specified slot.
        /// </summary>
        public async Task ConfirmBookingAsync(int slotId, int candidateId, int jobId)
        {
            var response = await this.http.PostAsJsonAsync(
                $"{BookingsPath}/{slotId}/confirm?jobId={jobId}", candidateId);
            response.EnsureSuccessStatusCode();
        }
    }
}
