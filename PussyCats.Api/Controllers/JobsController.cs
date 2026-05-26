using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Jobs;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService jobs;

    public JobsController(IJobService jobs)
    {
        this.jobs = jobs;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        if (companyId.HasValue)
            return Ok(await jobs.GetByCompanyIdAsync(companyId.Value, cancellationToken));
        return Ok(await jobs.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var job = await jobs.GetByIdAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Job job, CancellationToken cancellationToken)
    {
        job.JobId = 0;
        var saved = await jobs.AddAsync(job, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.JobId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Job job, CancellationToken cancellationToken)
    {
        if (await jobs.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        job.JobId = id;
        await jobs.UpdateAsync(job, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await jobs.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await jobs.RemoveAsync(id, cancellationToken);
        return NoContent();
    }
}
