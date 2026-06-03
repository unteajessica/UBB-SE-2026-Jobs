// <copyright file="SlotsApiClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Web.Clients
{
    using Microsoft.AspNetCore.Mvc;
    using PussyCats.Web.Dtos;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

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
        /// Retrieves all available (free) slots across all recruiters for a specific date.
        /// </summary>
        public async Task<List<SlotDto>> GetAvailableAsync(DateTime date)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            return await this.http.GetFromJsonAsync<List<SlotDto>>(
                $"{ApiPath}/available?date={formattedDate}") ?? new List<SlotDto>();
        }

        /// <summary>
        /// Retrieves all slots for a given recruiter.
        /// </summary>
        public async Task<List<SlotDto>> GetAllByRecruiterAsync(int recruiterId)
        {
            return await this.http.GetFromJsonAsync<List<SlotDto>>(
                $"{ApiPath}/recruiter/{recruiterId}") ?? new List<SlotDto>();
        }

        /// <summary>
        /// Retrieves all companies for a given recruiter.
        /// </summary>
        /// <param name="recruiterId"></param>
        /// <returns></returns>
        public async Task<List<CompanyDto>> GetCompaniesForRecruiterAsync(int recruiterId)
        {
           return await this.http.GetFromJsonAsync<List<CompanyDto>>($"api/companies/byrecruiter/{recruiterId}");
        }

        /// <summary>
        /// Retrieves all available for all recruiters of given company.
        /// </summary>
        public async Task<List<SlotDto>> GetAvailableSlotsForCompany(int companyId, DateTime date)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            return await this.http.GetFromJsonAsync<List<SlotDto>>(
                $"{ApiPath}/company/{companyId}?date={formattedDate}") ?? new List<SlotDto>();
        }

        /// <summary>
        /// Adds a new recruiter slot for a specific company.
        /// </summary>
        /// <param name="baseSlot">base slot with date, start time and company</param>
        /// <param name="duration">duration of slot</param>
        public async Task AddRecruiterSlotAsync(SlotDto baseSlot, int duration)
        {
            var payload = new { BaseSlot = baseSlot, Duration = duration };
            HttpResponseMessage response = await this.http.PostAsJsonAsync($"{ApiPath}/recruiter/create", payload);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Updates a new recruiter slot.
        /// </summary>
        /// <param name="initialSlot">the initial slot to modify</param>
        /// <param name="startime">the new start time</param>
        /// <param name="duration">the new duration time</param>
        public async Task UpdateRecruiterSlotAsync(SlotDto initialSlot, DateTime startime, int duration)
        {
            var payload = new { InitialSlot = initialSlot, StartTime = startime, Duration = duration };
            HttpResponseMessage response = await this.http.PutAsJsonAsync($"{ApiPath}/recruiter/update", payload);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Deletes a selected recruiter slot.
        /// </summary>
        /// <param name="slotId">id of slot to delete</param>
        public async Task DeleteRecruiterSlotAsync(int slotId)
        {
            HttpResponseMessage response = await this.http.DeleteAsync($"{ApiPath}/{slotId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
