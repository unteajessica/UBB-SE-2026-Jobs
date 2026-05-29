namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventsService _service;

        public EventsController(IEventsService service)
        {
            this._service = service;
        }

        [HttpPost]
        public ActionResult<EventDto> Add([FromBody] EventDto dto)
        {
            Event eventToBeAdded = dto.ToEntity();
            this._service.AddEventToRepo(eventToBeAdded);

            return Ok(eventToBeAdded.ToDto());
        }

        [HttpDelete("{id}")]
        public ActionResult Remove(int id)
        {
            this._service.RemoveEventFromRepo(new Event { Id = id });

            return Ok(new { message = "Event removed successfully" });
        }

        [HttpGet("current/{loggedInUser}")]
        public ActionResult<List<EventDto>> GetCurrent(int loggedInUser)
        {
            List<Event> events = this._service.GetCurrentEventsFromRepo(loggedInUser);

            if (events is null || !events.Any())
                return NotFound($"No current events found for user ID {loggedInUser}.");

            return Ok(events.Select(e => e.ToDto()).ToList());
        }

        [HttpGet("past/{loggedInUser}")]
        public ActionResult<List<EventDto>> GetPast(int loggedInUser)
        {
            List<Event> events = this._service.GetPastEventsFromRepo(loggedInUser);

            if (events is null || !events.Any())
                return NotFound($"No past events found for user ID {loggedInUser}.");

            return Ok(events.Select(e => e.ToDto()).ToList());
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] EventDto dto)
        {
            this._service.UpdateEventToRepo(id, dto.Photo, dto.Title, dto.Description, dto.StartDate, dto.EndDate, dto.Location);

            return Ok();
        }

        [HttpGet("current")]
        public IActionResult GetAllCurrentEvents()
        {
            var events = this._service.GetCurrentEventsFromRepo(null);
            return this.Ok(events);
        }

        [HttpGet("past")]
        public IActionResult GetAllPastEvents()
        {
            var events = this._service.GetPastEventsFromRepo(null);
            return this.Ok(events);
        }
    }
}