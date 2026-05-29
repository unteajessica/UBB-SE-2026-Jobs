// <copyright file="InterviewsController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using PussyCats.Web.Clients;
    using PussyCats.Web.Dtos;

    /// <summary>
    /// Handles interview slot browsing and booking (candidates) and
    /// booked interview management (recruiters).
    /// </summary>
    public class InterviewsController : Controller
    {
        private readonly SlotsApiClient slotsClient;
        private readonly InterviewSessionsApiClient sessionsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewsController"/> class.
        /// </summary>
        public InterviewsController(
            SlotsApiClient slotsClient,
            InterviewSessionsApiClient sessionsClient)
        {
            this.slotsClient = slotsClient;
            this.sessionsClient = sessionsClient;
        }

        // ---------------------------------------------------------------
        // Candidate: browse available slots
        // ---------------------------------------------------------------

        /// <summary>
        /// Displays available interview slots for a recruiter on a given date.
        /// Accessible by candidates only.
        /// </summary>
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> AvailableSlots(DateTime? date)
        {
            DateTime selectedDate = date ?? DateTime.Today;
            var slots = await this.slotsClient.GetAvailableAsync(selectedDate);
            this.ViewBag.SelectedDate = selectedDate;
            return this.View(slots);
        }

        // ---------------------------------------------------------------
        // Candidate: book a slot
        // ---------------------------------------------------------------

        /// <summary>
        /// Confirms the booking of a slot for the currently logged-in candidate.
        /// Accessible by candidates only.
        /// </summary>
        [Authorize(Roles = "Candidate")]
        [HttpPost]
        public async Task<IActionResult> BookSlot(int slotId, int recruiterId)
        {
            int candidateId = int.Parse(
                this.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await this.sessionsClient.ConfirmBookingAsync(slotId, candidateId);
                this.TempData["Success"] = "Interview slot booked successfully!";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                this.TempData["Error"] = "This slot is no longer available.";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                this.TempData["Error"] = "Slot not found.";
            }

            return this.RedirectToAction(nameof(this.AvailableSlots));
        }

        // ---------------------------------------------------------------
        // Recruiter: view booked interviews
        // ---------------------------------------------------------------

        /// <summary>
        /// Displays all scheduled interview sessions for the recruiter.
        /// Accessible by recruiters only.
        /// </summary>
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> BookedInterviews()
        {
            var sessions = await this.sessionsClient.GetScheduledAsync();
            return this.View(sessions);
        }
    }
}
