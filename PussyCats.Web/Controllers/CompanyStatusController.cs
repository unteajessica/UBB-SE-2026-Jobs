using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.Matches;
using PussyCats.Web.Configuration;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

//[Authorize] // uncomment once Dev 1 lands the auth scaffolding (Assignment 5 section A.3)
public class CompanyStatusController : Controller
{
    private readonly ICompanyStatusService companyStatusService;
    private readonly IMatchService matchService;
    private readonly ApiConfiguration apiConfiguration;

    public CompanyStatusController(
        ICompanyStatusService companyStatusService,
        IMatchService matchService,
        ApiConfiguration apiConfiguration)
    {
        this.companyStatusService = companyStatusService;
        this.matchService = matchService;
        this.apiConfiguration = apiConfiguration;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = apiConfiguration.TemporaryCompanyId;
        var applicants = await companyStatusService
            .GetApplicantsForCompanyAsync(companyId, cancellationToken);
        return View(applicants);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var companyId = apiConfiguration.TemporaryCompanyId;
        var applicant = await companyStatusService
            .GetApplicantByMatchIdAsync(companyId, id, cancellationToken);
        return applicant is null ? NotFound() : View(applicant);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(UserApplicationResult model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var companyId = apiConfiguration.TemporaryCompanyId;
        var applicant = await companyStatusService
            .GetApplicantByMatchIdAsync(companyId, id, cancellationToken);
        if (applicant is null)
        {
            return NotFound();
        }

        var model = new MatchDecisionFormModel
        {
            MatchId = applicant.Match.MatchId,
            Decision = applicant.Match.Status,
            Feedback = applicant.Match.FeedbackMessage,
            ApplicantName = applicant.User.Name,
            JobTitle = applicant.Job.JobTitle,
            CompanyName = applicant.Job.Company.CompanyName,
            CurrentStatus = applicant.Match.Status,
            Timestamp = applicant.Match.Timestamp,
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MatchDecisionFormModel model, CancellationToken cancellationToken)
    {
        if (id != model.MatchId)
        {
            return BadRequest();
        }

        if (!TryValidateDecision(model))
        {
            return View(model);
        }

        await matchService.SubmitDecisionAsync(
            model.MatchId,
            model.Decision!.Value,
            model.Feedback.Trim(),
            cancellationToken);

        return RedirectToAction(nameof(Details), new { id = model.MatchId });
    }

    private bool TryValidateDecision(MatchDecisionFormModel model)
    {
        if (model.Decision is not (PussyCats.Library.Domain.Enums.MatchStatus.Accepted
            or PussyCats.Library.Domain.Enums.MatchStatus.Rejected))
        {
            ModelState.AddModelError(nameof(model.Decision), "Select either Accepted or Rejected.");
        }

        if (string.IsNullOrWhiteSpace(model.Feedback))
        {
            ModelState.AddModelError(nameof(model.Feedback), "Feedback is required.");
        }
        else if (model.Feedback.Trim().Length > MatchDecisionFormModel.MaximumFeedbackLength)
        {
            ModelState.AddModelError(nameof(model.Feedback), "Feedback must be 500 characters or fewer.");
        }

        return ModelState.IsValid;
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var companyId = apiConfiguration.TemporaryCompanyId;
        var applicant = await companyStatusService
            .GetApplicantByMatchIdAsync(companyId, id, cancellationToken);
        return applicant is null ? NotFound() : View(applicant);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        return RedirectToAction(nameof(Index));
    }
}
