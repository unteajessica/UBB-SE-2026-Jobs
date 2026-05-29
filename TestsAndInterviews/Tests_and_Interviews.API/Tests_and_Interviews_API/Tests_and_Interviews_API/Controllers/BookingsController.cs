namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Handles HTTP requests related to bookings and interview slot reservations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingsController"/> class.
        /// </summary>
        /// <param name="bookingService">The booking service to be used by the controller.</param>
        public BookingsController(IBookingService bookingService)
        {
            this._bookingService = bookingService;
        }

        /// <summary>
        /// Confirms a booking for a candidate by reserving an interview slot.
        /// </summary>
        /// <param name="slotId">The unique identifier of the slot to be booked.</param>
        /// <param name="candidateId">The unique identifier of the candidate making the booking.</param>
        /// <returns>An <see cref="OkResult"/> if the booking was confirmed successfully.</returns>
        /// <response code="200">The booking was confirmed successfully.</response>
        /// <response code="404">The slot was not found.</response>
        /// <response code="409">The slot is no longer available.</response>
        [HttpPost("{slotId}/confirm")]
        public async Task<ActionResult> ConfirmBooking(int slotId, [FromBody] int candidateId)
        {
            try
            {
                await this._bookingService.ConfirmBookingAsync(slotId, candidateId);
                return Ok();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return Conflict(e.Message);
            }
        }
    }
}
