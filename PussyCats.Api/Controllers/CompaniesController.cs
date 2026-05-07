using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Jobs;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyRepository companies;
    private readonly IJobRepository jobs;

    public CompaniesController(ICompanyRepository companies, IJobRepository jobs)
    {
        this.companies = companies;
        this.jobs = jobs;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await companies.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var company = await companies.GetByIdAsync(id, cancellationToken);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpGet("{id}/jobs")]
    public async Task<IActionResult> GetJobs(int id, CancellationToken cancellationToken)
    {
        if (await companies.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        return Ok(await jobs.GetByCompanyIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Company company, CancellationToken ct)
    {
        company.CompanyId = 0;
        var saved = await companies.AddAsync(company, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.CompanyId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Company company, CancellationToken cancellationToken)
    {
        if (await companies.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        company.CompanyId = id;
        await companies.UpdateAsync(company, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await companies.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await companies.RemoveAsync(id, cancellationToken);
        return NoContent();
    }
}
