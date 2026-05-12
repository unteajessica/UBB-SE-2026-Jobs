using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.PersonalityTests;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/personality-tests")]
public class PersonalityTestsController : ControllerBase
{
    private readonly IPersonalityTestRepository personalityTests;

    public PersonalityTestsController(IPersonalityTestRepository personalityTests)
    {
        this.personalityTests = personalityTests;
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] int userId, CancellationToken cancellationToken)
    {
        var result = await personalityTests.GetByUserIdAsync(userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] PersonalityTestResult result, CancellationToken cancellationToken)
    {
        if (result.User == null)
        {
            return BadRequest("User navigation property was not provided or failed to deserialize.");
        }
        var saved = await personalityTests.AddAsync(result, cancellationToken);
        return CreatedAtAction(nameof(GetByUserId), new { userId = saved.User.UserId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PersonalityTestResult result, CancellationToken cancellationToken)
    {
        // IPersonalityTestRepository has no GetByIdAsync; existence is not pre-checked.
        // EF will throw DbUpdateConcurrencyException if the row is missing.
        result.PersonalityTestResultId = id;
        await personalityTests.UpdateAsync(result, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        await personalityTests.RemoveAsync(id, cancellationToken);
        return NoContent();
    }
}
