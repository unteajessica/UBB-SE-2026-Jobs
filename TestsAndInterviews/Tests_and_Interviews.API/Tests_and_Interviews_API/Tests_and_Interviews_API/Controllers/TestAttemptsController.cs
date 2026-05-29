namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class TestAttemptsController : ControllerBase
    {
        private readonly ITestAttemptService _service;

        public TestAttemptsController(ITestAttemptService service)
        {
            this._service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestAttemptDto>> FindById(int id)
        {
            TestAttempt? attempt = await this._service.FindByIdAsync(id);

            if (attempt == null)
            {
                return NotFound();
            }

            return Ok(attempt.ToDto());
        }

        [HttpGet("byuser/{userId}/bytest/{testId}")]
        public async Task<ActionResult<TestAttemptDto>> FindByUserAndTest(int userId, int testId)
        {
            TestAttempt? attempt = await this._service.FindByUserAndTestAsync(userId, testId);

            if (attempt == null)
            {
                return NotFound();
            }

            return Ok(attempt.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult> Save([FromBody] TestAttemptDto dto)
        {
            await this._service.SaveAsync(dto.ToEntity());

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TestAttemptDto>> Update(int id, [FromBody] TestAttemptDto dto)
        {
            TestAttempt attempt = dto.ToEntity();
            attempt.Id = id;
            TestAttempt? updated = await this._service.UpdateAsync(attempt);

            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated.ToDto());
        }

        [HttpGet("valid/bytest/{testId}")]
        public async Task<ActionResult<List<TestAttemptDto>>> FindValidAttemptsByTestId(int testId)
        {
            List<TestAttempt> attempts = await this._service.FindValidAttemptsByTestIdAsync(testId);

            return Ok(attempts.Select(a => a.ToDto()).ToList());
        }
    }
}