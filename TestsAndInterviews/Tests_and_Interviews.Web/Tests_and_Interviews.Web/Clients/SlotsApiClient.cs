// <copyright file="SlotsApiClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Web.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Web.Dtos;

    /// <summary>
    /// HTTP client for the Slots API endpoints, used by the MVC web app.
    /// </summary>
    public class SlotsApiClient
    {
        private readonly HttpClient http;
        private const string ApiPath = "api/slots";

        public SlotsApiClient(HttpClient http)
        {
            this.http = http;
        }

        /// <summary>
        /// Retrieves available slots for a given recruiter on a specific date.
        /// </summary>
        public async Task<List<SlotDto>> GetAvailableAsync(int recruiterId, DateTime date)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            return await this.http.GetFromJsonAsync<List<SlotDto>>(
                $"{ApiPath}/recruiter/{recruiterId}/date?date={formattedDate}") ?? new List<SlotDto>();
        }

        /// <summary>
        /// Retrieves all slots for a given recruiter.
        /// </summary>
        public async Task<List<SlotDto>> GetAllByRecruiterAsync(int recruiterId)
        {
            return await this.http.GetFromJsonAsync<List<SlotDto>>(
                $"{ApiPath}/recruiter/{recruiterId}") ?? new List<SlotDto>();
        }
    }
}