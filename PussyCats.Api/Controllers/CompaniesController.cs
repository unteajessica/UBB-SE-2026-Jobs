using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Helpers;
using PussyCats.Library.Services.CompanyService;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/companies")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService companyService;

    public CompanyController(ICompanyService companyService)
    {
        this.companyService = companyService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Company>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var companies = await companyService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Ok(companies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Company), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        var company = await companyService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        DebugToFile.Write("Controller",$"GetByIdAsync: Retrieved company with ID {id}: {(company != null ? company.CompanyName : "Not Found")}");
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Company), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAsync([FromBody] Company company, CancellationToken cancellationToken)
    {
        var createdCompany = await companyService.AddAsync(company, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetById", new { id = createdCompany.CompanyId }, createdCompany);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] Company company, CancellationToken cancellationToken)
    {
        if (id != company.CompanyId)
            return BadRequest("Route id does not match the company id in the request body.");

        await companyService.UpdateAsync(company, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        await companyService.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
