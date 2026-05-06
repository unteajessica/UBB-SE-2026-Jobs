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
    public async Task<IActionResult> GetByUserId([FromQuery] int userId, CancellationToken ct)
    {
        var result = await personalityTests.GetByUserIdAsync(userId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] PersonalityTestResult result, CancellationToken ct)
    {
        var saved = await personalityTests.AddAsync(result, ct);
        return CreatedAtAction(nameof(GetByUserId), new { userId = saved.UserId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PersonalityTestResult result, CancellationToken ct)
    {
        // IPersonalityTestRepository has no GetByIdAsync; existence is not pre-checked.
        // EF will throw DbUpdateConcurrencyException if the row is missing.
        result.PersonalityTestResultId = id;
        await personalityTests.UpdateAsync(result, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        await personalityTests.RemoveAsync(id, ct);
        return NoContent();
    }
}
