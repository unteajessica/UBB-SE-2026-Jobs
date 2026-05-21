using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.CompanyRecommendationService;
using PussyCats.Web.Configuration;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

[Authorize]
public class CompanyRecommendationsController : Controller
{
    private readonly ICompanyRecommendationService companyRecommendations;
    private readonly ApiConfiguration apiConfiguration;

    public CompanyRecommendationsController(
        ICompanyRecommendationService companyRecommendations,
        ApiConfiguration apiConfiguration)
    {
        this.companyRecommendations = companyRecommendations;
        this.apiConfiguration = apiConfiguration;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var applicants = await companyRecommendations.GetRankedApplicantsAsync(
            apiConfiguration.TemporaryCompanyId,
            cancellationToken);
        return View(applicants);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var applicant = await companyRecommendations.GetApplicantByMatchIdAsync(
            apiConfiguration.TemporaryCompanyId,
            id,
            cancellationToken);

        if (applicant is null)
        {
            return NotFound();
        }

        var breakdown = await companyRecommendations.GetBreakdownAsync(applicant, cancellationToken);
        return View(new CompanyRecommendationDetailsModel
        {
            Applicant = applicant,
            Breakdown = breakdown,
        });
    }
}
