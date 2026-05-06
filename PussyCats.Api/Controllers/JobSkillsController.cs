using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/jobs/{jobId}/skills")]
public class JobSkillsController : ControllerBase
{
    private readonly IJobSkillRepository jobSkills;

    public JobSkillsController(IJobSkillRepository jobSkills)
    {
        this.jobSkills = jobSkills;
    }

    [HttpGet("/api/job-skills")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await jobSkills.GetAllAsync(ct));

    [HttpGet]
    public async Task<IActionResult> GetByJobId(int jobId, CancellationToken ct)
        => Ok(await jobSkills.GetByJobIdAsync(jobId, ct));

    [HttpGet("{skillId}")]
    public async Task<IActionResult> GetBySkillId(int jobId, int skillId, CancellationToken ct)
    {
        var jobSkill = await jobSkills.GetAsync(jobId, skillId, ct);
        return jobSkill is null ? NotFound() : Ok(jobSkill);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int jobId, [FromBody] JobSkill jobSkill, CancellationToken ct)
    {
        jobSkill.JobId = jobId;
        var saved = await jobSkills.AddAsync(jobSkill, ct);
        return CreatedAtAction(nameof(GetBySkillId), new { jobId, skillId = saved.SkillId }, saved);
    }

    [HttpPut("{skillId}")]
    public async Task<IActionResult> Update(int jobId, int skillId, [FromBody] JobSkill jobSkill, CancellationToken ct)
    {
        if (await jobSkills.GetAsync(jobId, skillId, ct) is null)
            return NotFound();
        jobSkill.JobId = jobId;
        jobSkill.SkillId = skillId;
        await jobSkills.UpdateAsync(jobSkill, ct);
        return NoContent();
    }

    [HttpDelete("{skillId}")]
    public async Task<IActionResult> Remove(int jobId, int skillId, CancellationToken ct)
    {
        if (await jobSkills.GetAsync(jobId, skillId, ct) is null)
            return NotFound();
        await jobSkills.RemoveAsync(jobId, skillId, ct);
        return NoContent();
    }
}
