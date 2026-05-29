// <copyright file="SlotService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing recruiter slots, including retrieval and creation of slots.
    /// </summary>
    public class SlotService : ISlotService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotService"/> class.
        /// </summary>
        public SlotService()
        {
            this.http = ApiClient.Http;
        }

        public SlotService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        /// <summary>
        /// Asynchronously retrieves a list of slots for the specified recruiter and date, including both
        /// existing free and occupied slots and free 30-minutes slots between 8:00 and 18:00.
        /// </summary>
        /// <param name="recruitedId">The unique identifier of the recruiter.</param>
        /// <param name="date">The date for which to load the slots.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of slot DTOs for the specified
        /// recruiter and date.</returns>
        public async Task<List<SlotDto>> LoadRecruiterVisibleSlotsAsync(
     int recruiterId, DateTime date)
        {
            string url = $"api/slots/recruiter/{recruiterId}/visible?date={Uri.EscapeDataString(date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}";

            try
            {
                HttpResponseMessage response = await this.http.GetAsync(url);

                if (!response.IsSuccessStatusCode) { return new List<SlotDto>(); }

                List<SlotDto>? slots = await response.Content.ReadFromJsonAsync<List<SlotDto>>();

                if (slots == null)  return new List<SlotDto>();

                foreach (SlotDto slot in slots)
                {
                    slot.StartTime = slot.StartTime.ToLocalTime();
                    slot.EndTime = slot.EndTime.ToLocalTime(); }

                return slots;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SlotService] Exception: {ex.Message}");
                return new List<SlotDto>();
            }
        }

        /// <summary>
        /// Creates a new recruiter slot with the specified start time and duration.
        /// </summary>
        /// <param name="baseSlot">The base slot containing the start time.</param>
        /// <param name="duration">The duration of the slot in minutes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CreateRecruiterSlotAsync(SlotDto baseSlot, int duration)
        {
            var payload = new { BaseSlot = baseSlot, Duration = duration };
            HttpResponseMessage response = await this.http.PostAsJsonAsync("api/slots/recruiter/create", payload);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Asynchronously deletes a recruiter slot with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the recruiter slot to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeleteRecruiterSlotAsync(int id)
        {
            HttpResponseMessage response = await this.http.DeleteAsync($"api/slots/{id}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Updates a recruiter's slot with new start time and duration asynchronously.
        /// </summary>
        /// <param name="initialSlot">The initial slot to update.</param>
        /// <param name="startTime">The new start time for the slot.</param>
        /// <param name="duration">The new duration of the slot in minutes.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <exception cref="Exception">Thrown when the start time is outside the allowed hours of 8 to 18.</exception>
        public async Task UpdateRecruiterSlotAsync(SlotDto initialSlot, DateTime startTime, int duration)
        {
            var payload = new { InitialSlot = initialSlot, StartTime = startTime, Duration = duration };
            HttpResponseMessage response = await this.http.PutAsJsonAsync("api/slots/recruiter/update", payload);
            response.EnsureSuccessStatusCode();
        }
    }
}