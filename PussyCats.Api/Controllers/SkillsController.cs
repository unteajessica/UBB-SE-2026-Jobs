using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Skills;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService skills;

    public SkillsController(ISkillService skills)
    {
        this.skills = skills;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await skills.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var skill = await skills.GetByIdAsync(id, cancellationToken);
        return skill is null ? NotFound() : Ok(skill);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Skill skill, CancellationToken cancellationToken)
    {
        skill.SkillId = 0;
        var saved = await skills.AddAsync(skill, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.SkillId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Skill skill, CancellationToken cancellationToken)
    {
        if (await skills.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        skill.SkillId = id;
        await skills.UpdateAsync(skill, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await skills.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        try
        {
            await skills.RemoveAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }
}
