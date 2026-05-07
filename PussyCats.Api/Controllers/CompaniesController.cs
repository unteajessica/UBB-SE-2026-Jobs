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
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await companies.GetAllAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var company = await companies.GetByIdAsync(id, ct);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpGet("{id}/jobs")]
    public async Task<IActionResult> GetJobs(int id, CancellationToken ct)
    {
        if (await companies.GetByIdAsync(id, ct) is null)
            return NotFound();
        return Ok(await jobs.GetByCompanyIdAsync(id, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Company company, CancellationToken ct)
    {
        company.CompanyId = 0;
        var saved = await companies.AddAsync(company, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.CompanyId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Company company, CancellationToken ct)
    {
        if (await companies.GetByIdAsync(id, ct) is null)
            return NotFound();
        company.CompanyId = id;
        await companies.UpdateAsync(company, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        if (await companies.GetByIdAsync(id, ct) is null)
            return NotFound();
        await companies.RemoveAsync(id, ct);
        return NoContent();
    }
}
