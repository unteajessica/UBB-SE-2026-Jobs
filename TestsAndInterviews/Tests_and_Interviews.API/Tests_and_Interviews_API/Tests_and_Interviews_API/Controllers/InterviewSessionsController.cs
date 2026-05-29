namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class InterviewSessionsController : ControllerBase
    {
        private readonly IInterviewSessionService _service;

        public InterviewSessionsController(IInterviewSessionService service)
        {
            this._service = service;
        }

        [HttpGet("scheduled")]
        public async Task<ActionResult<List<InterviewSessionDto>>> GetScheduled()
        {
            List<InterviewSession> sessions = await this._service.GetScheduledSessionsAsync();

            return Ok(sessions.Select(session => session.ToDto(Request)).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InterviewSessionDto>> GetById(int id)
        {
            try
            {
                InterviewSession session = await this._service.GetInterviewByIdAsync(id);

                return Ok(session.ToDto(Request));
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<InterviewSessionDto>>> GetByStatus(string status)
        {
            List<InterviewSession> sessions = await this._service.GetInterviewsByStatusAsync(status);

            return Ok(sessions.Select(session => session.ToDto(Request)).ToList());
        }

        [HttpPost()]
        public async Task<ActionResult<InterviewSessionDto>> Create([FromBody] InterviewSessionDto dto)
        {
            InterviewSession created = await this._service.AddInterviewAsync(dto.ToEntity());

            return Ok(created.ToDto(Request));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<InterviewSessionDto>> Update(int id, [FromBody] InterviewSessionDto dto)
        {
            try
            {
                InterviewSession updated = await this._service.UpdateInterviewAsync(id, dto.ToEntity());

                return Ok(updated.ToDto(Request));
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await this._service.DeleteInterviewAsync(id);

                if (deleted)
                {
                    return Ok(new { message = "Interview session deleted successfully" });
                }

                return BadRequest();
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{sessionId}/video")]
        public async Task<ActionResult<InterviewSessionDto>> UploadVideo(int sessionId, [FromForm] VideoUploadDto video)
        {
            try
            {
                if (video.File == null || video.File.Length == 0)
                {
                    return BadRequest("No video uploaded");
                }

                InterviewSession session = await this._service.UploadVideoAsync(sessionId, video.File);

                return Ok(session.ToDto(Request));
            } catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("videos/{videoName}")]
        public async Task<ActionResult> GetVideo(string videoName)
        {
            try
            {
                (byte[] videoBytes, string contentType) = await this._service.GetVideoAsync(videoName);

                return File(videoBytes, contentType);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
