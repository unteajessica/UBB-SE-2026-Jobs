using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.CompanyRecommendationService;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/company-recommendations")]
public class CompanyRecommendationsController : ControllerBase
{
    private readonly ICompanyRecommendationService companyRecommendations;

    public CompanyRecommendationsController(ICompanyRecommendationService companyRecommendations)
    {
        this.companyRecommendations = companyRecommendations;
    }

    [HttpGet("companies/{companyId}/applicants")]
    public async Task<IActionResult> GetRankedApplicants(int companyId, CancellationToken cancellationToken)
    {
        return Ok(await companyRecommendations.GetRankedApplicantsAsync(companyId, cancellationToken));
    }

    [HttpGet("companies/{companyId}/applicants/{matchId}")]
    public async Task<IActionResult> GetApplicantByMatchId(int companyId, int matchId, CancellationToken cancellationToken)
    {
        var applicant = await companyRecommendations.GetApplicantByMatchIdAsync(companyId, matchId, cancellationToken);
        return applicant is null ? NotFound() : Ok(applicant);
    }

    [HttpPost("breakdown")]
    public async Task<IActionResult> GetBreakdown([FromBody] UserApplicationResult applicant, CancellationToken cancellationToken)
    {
        if (applicant is null)
        {
            return BadRequest();
        }

        var breakdown = await companyRecommendations.GetBreakdownAsync(applicant, cancellationToken);
        return breakdown is null ? NotFound() : Ok(breakdown);
    }
}
