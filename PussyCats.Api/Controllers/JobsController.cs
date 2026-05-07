using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository jobs;

    public JobsController(IJobRepository jobs)
    {
        this.jobs = jobs;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, CancellationToken ct)
    {
        if (companyId.HasValue)
            return Ok(await jobs.GetByCompanyIdAsync(companyId.Value, ct));
        return Ok(await jobs.GetAllAsync(ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var job = await jobs.GetByIdAsync(id, ct);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Job job, CancellationToken ct)
    {
        job.JobId = 0;
        var saved = await jobs.AddAsync(job, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.JobId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Job job, CancellationToken ct)
    {
        if (await jobs.GetByIdAsync(id, ct) is null)
            return NotFound();
        job.JobId = id;
        await jobs.UpdateAsync(job, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        if (await jobs.GetByIdAsync(id, ct) is null)
            return NotFound();
        await jobs.RemoveAsync(id, ct);
        return NoContent();
    }
}
