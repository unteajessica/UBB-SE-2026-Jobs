using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.CompanyStatusService;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/company-status")]
public class CompanyStatusController : ControllerBase
{
    private readonly ICompanyStatusService companyStatusService;

    public CompanyStatusController(ICompanyStatusService companyStatusService)
    {
        this.companyStatusService = companyStatusService;
    }

    [HttpGet("companies/{companyId}/applicants")]
    public async Task<IActionResult> GetApplicantsForCompany(int companyId, CancellationToken cancellationToken)
    {
        var applicants = await companyStatusService.GetApplicantsForCompanyAsync(companyId, cancellationToken);
        return Ok(applicants);
    }

    [HttpGet("companies/{companyId}/applicants/{matchId}")]
    public async Task<IActionResult> GetApplicantByMatchId(int companyId, int matchId, CancellationToken cancellationToken)
    {
        var applicant = await companyStatusService.GetApplicantByMatchIdAsync(companyId, matchId, cancellationToken);
        return applicant is null ? NotFound() : Ok(applicant);
    }
}
