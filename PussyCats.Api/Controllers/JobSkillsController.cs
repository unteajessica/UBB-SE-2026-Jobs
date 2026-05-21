using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.JobSkills;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/jobs/{jobId}/skills")]
public class JobSkillsController : ControllerBase
{
    private readonly IJobSkillService jobSkills;

    public JobSkillsController(IJobSkillService jobSkills)
    {
        this.jobSkills = jobSkills;
    }

    [HttpGet("/api/job-skills")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await jobSkills.GetAllAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> GetByJobId(int jobId, CancellationToken cancellationToken)
        => Ok(await jobSkills.GetByJobIdAsync(jobId, cancellationToken));

    [HttpGet("{skillId}")]
    public async Task<IActionResult> GetBySkillId(int jobId, int skillId, CancellationToken cancellationToken)
    {
        var jobSkill = await jobSkills.GetByIdAsync(jobId, skillId, cancellationToken);
        return jobSkill is null ? NotFound() : Ok(jobSkill);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int jobId, [FromBody] JobSkill jobSkill, CancellationToken cancellationToken)
    {
        jobSkill.Job = new Job { JobId = jobId };
        var saved = await jobSkills.AddAsync(jobSkill, cancellationToken);
        return CreatedAtAction(nameof(GetBySkillId), new { jobId, skillId = saved.Skill.SkillId }, saved);
    }

    [HttpPut("{skillId}")]
    public async Task<IActionResult> Update(int jobId, int skillId, [FromBody] JobSkill jobSkill, CancellationToken cancellationToken)
    {
        if (await jobSkills.GetByIdAsync(jobId, skillId, cancellationToken) is null)
            return NotFound();
        jobSkill.Job = new Job { JobId = jobId };
        jobSkill.Skill = new Skill { SkillId = skillId };
        await jobSkills.UpdateAsync(jobSkill, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{skillId}")]
    public async Task<IActionResult> Remove(int jobId, int skillId, CancellationToken cancellationToken)
    {
        if (await jobSkills.GetByIdAsync(jobId, skillId, cancellationToken) is null)
            return NotFound();
        await jobSkills.RemoveAsync(jobId, skillId, cancellationToken);
        return NoContent();
    }
}
