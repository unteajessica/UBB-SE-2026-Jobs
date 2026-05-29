namespace PussyCats.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using PussyCats.Web.Clients;
    using PussyCats.Web.Dtos;

    [Authorize]
    public class EventsController : Controller
    {
        private readonly EventsApiClient _client;
        public EventsController(EventsApiClient client)
        {
            this._client = client;
        }

        private int GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public async Task<IActionResult> Index()
        {
            List<EventDto> currentEvents;
            List<EventDto> pastEvents;

            if (User.IsInRole("Candidate"))
            {
                currentEvents = await this._client.GetAllCurrentEvents();
                pastEvents = await this._client.GetAllPastEvents();
            }
            else
            {
                int companyId = GetCurrentUserId(); // temporary because your recruiter user id == company id in local DB
                currentEvents = await this._client.GetCurrentEvents(companyId);
                pastEvents = await this._client.GetPastEvents(companyId);
            }

            ViewBag.CurrentEvents = currentEvents;
            ViewBag.PastEvents = pastEvents;

            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            List<EventDto> currentEvents;
            List<EventDto> pastEvents;

            if (User.IsInRole("Candidate"))
            {
                currentEvents = await this._client.GetAllCurrentEvents();
                pastEvents = await this._client.GetAllPastEvents();
            }
            else
            {
                int companyId = GetCurrentUserId(); // temporary because recruiter user id = company id in your local DB
                currentEvents = await this._client.GetCurrentEvents(companyId);
                pastEvents = await this._client.GetPastEvents(companyId);
            }

            EventDto? ev = currentEvents
                .Concat(pastEvents)
                .FirstOrDefault(e => e.Id == id);

            if (ev == null)
            {
                return NotFound();
            }

            List<CollaboratorDto> collaborators = await this._client.GetCollaborators(ev.HostCompanyId);
            ViewBag.Collaborators = collaborators;

            return View(ev);
        }

        [Authorize(Policy = "RecruiterOrAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "RecruiterOrAdmin")]
        public async Task<IActionResult> Create(EventDto dto)
        {
            dto.HostCompanyId = GetCurrentUserId();
            dto.PostedAt = DateTime.Now;
            await this._client.Create(dto);
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "RecruiterOrAdmin")]
        public async Task<IActionResult> Edit(int id)
        {
            int userId = GetCurrentUserId();
            List<EventDto> currentEvents = await this._client.GetCurrentEvents(userId);
            List<EventDto> pastEvents = await this._client.GetPastEvents(userId);

            EventDto? ev = currentEvents.Concat(pastEvents).FirstOrDefault(e => e.Id == id);

            if (ev == null)
                return NotFound();

            return View(ev);
        }

        [HttpPost]
        [Authorize(Policy = "RecruiterOrAdmin")]
        public async Task<IActionResult> Edit(int id, EventDto dto)
        {
            await this._client.Update(id, dto);
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "RecruiterOrAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = GetCurrentUserId();
            List<EventDto> currentEvents = await this._client.GetCurrentEvents(userId);
            List<EventDto> pastEvents = await this._client.GetPastEvents(userId);

            EventDto? ev = currentEvents.Concat(pastEvents).FirstOrDefault(e => e.Id == id);

            if (ev == null)
                return NotFound();

            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "RecruiterOrAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await this._client.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
