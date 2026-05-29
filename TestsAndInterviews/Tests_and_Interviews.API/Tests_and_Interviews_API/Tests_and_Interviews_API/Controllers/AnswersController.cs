namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _service;

        public AnswersController(IAnswerService service)
        {
            this._service = service;
        }

        [HttpPost]
        public async Task<ActionResult> Save([FromBody] AnswerDto dto)
        {
            await this._service.SaveAsync(dto.ToEntity());

            return Ok();
        }

        [HttpGet("byattempt/{attemptId}")]
        public async Task<ActionResult<List<AnswerDto>>> FindByAttempt(int attemptId)
        {
            List<Answer> answers = await this._service.FindByAttemptAsync(attemptId);

            if (answers is null || !answers.Any())
                return NotFound($"No answers found for attempt ID {attemptId}.");

            return Ok(answers.Select(a => a.ToDto()).ToList());
        }
    }
}