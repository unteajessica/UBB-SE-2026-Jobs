// <copyright file="BookingService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// Provides booking-related operations for managing interview slot reservations and session creation.
    /// </summary>
    public class BookingService : IBookingService
    {
        private const int MINIMUMPOSITIONID = 0;
        private const int MINIMUMINTERVIEWSCORE = 0;
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingService"/> class.
        /// </summary>
        public BookingService()
        {
            this.http = ApiClient.Http;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        public BookingService(HttpClient httpClient)
        {
            this.http = httpClient ?? ApiClient.Http;
        }

        /// <summary>
        /// Gets the available slots for a given recruiter at a given date.
        /// </summary>
        /// <param name="recruiterId"> Id of the recruiter.</param>
        /// <param name="date"> The date for which to retrieve available slots.</param>
        /// <returns> A list of available slots for the specified recruiter and date.</returns>
        public async Task<List<Slot>> GetAvailableSlots(int recruiterId, DateTime date)
        {
            HttpResponseMessage response = await this.http.GetAsync($"slots/recruiter/{recruiterId}/date?date={date:O}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Slot>();
            }

            response.EnsureSuccessStatusCode();
            List<SlotDto>? dtos = await response.Content.ReadFromJsonAsync<List<SlotDto>>();
            return dtos?
                .Select(dto => dto.ToEntity())
                .Where(slot => slot.Status == SlotStatus.Free)
                .OrderBy(slot => slot.StartTime)
                .ToList() ?? new List<Slot>();
        }

        /// <summary>
        /// Gets all available slots for a given recruiter, regardless of the date.
        /// </summary>
        /// <param name="recruiterId"> Id of the recruiter.</param>
        /// <returns> A list of all available slots for the specified recruiter.</returns>
        public async Task<List<Slot>> GetAvailableSlotsByRecruiterId(int recruiterId)
        {
            HttpResponseMessage response = await this.http.GetAsync($"slots/recruiter/{recruiterId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Slot>();
            }

            response.EnsureSuccessStatusCode();
            List<SlotDto>? dtos = await response.Content.ReadFromJsonAsync<List<SlotDto>>();
            return dtos?
                .Select(dto => dto.ToEntity())
                .Where(slot => slot.Status == SlotStatus.Free)
                .OrderBy(slot => slot.StartTime)
                .ToList() ?? new List<Slot>();
        }

        /// <summary>
        /// Confirms a booking for a candidate by updating the slot's status to occupied and creating a new interview session.
        /// </summary>
        /// <param name="candidateId"> Id of the candidate.</param>
        /// <param name="slot"> The slot to be booked.</param>
        /// <exception cref="Exception"> Thrown when the slot is not found or is no longer available.</exception>
        public async Task ConfirmBooking(int candidateId, Slot slot)
        {
            HttpResponseMessage response = await this.http.PostAsJsonAsync($"bookings/{slot.Id}/confirm", candidateId);
            response.EnsureSuccessStatusCode();
        }
    }
}