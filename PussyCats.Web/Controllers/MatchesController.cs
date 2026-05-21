using System.Net;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Matches;
using PussyCats.Web.Configuration;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

//[Authorize] // uncomment once Dev 1 lands the auth scaffolding (Assignment 5 section A.3)
public class MatchesController : Controller
{
    private readonly IMatchService matches;
    private readonly ApiConfiguration apiConfiguration;

    public MatchesController(IMatchService matches, ApiConfiguration apiConfiguration)
    {
        this.matches = matches;
        this.apiConfiguration = apiConfiguration;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyMatches = await matches.GetByCompanyIdAsync(apiConfiguration.TemporaryCompanyId, cancellationToken);
        return View(companyMatches);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(id, cancellationToken);
        return match is null ? NotFound() : View(match);
    }

    public async Task<IActionResult> Decision(int id, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(id, cancellationToken);
        return match is null ? NotFound() : View(CreateDecisionForm(match));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Decision(int id, MatchDecisionFormModel model, CancellationToken cancellationToken)
    {
        if (id != model.MatchId)
            return BadRequest();

        ValidateDecisionModel(model);

        if (!ModelState.IsValid)
        {
            if (!await PopulateDecisionDisplayFieldsAsync(model, cancellationToken))
                return NotFound();

            return View(model);
        }

        try
        {
            await matches.SubmitDecisionAsync(
                model.MatchId,
                model.Decision!.Value,
                model.Feedback.Trim(),
                cancellationToken);
            return RedirectToAction(nameof(Details), new { id = model.MatchId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (Exception exception) when (IsDecisionValidationException(exception))
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            if (!await PopulateDecisionDisplayFieldsAsync(model, cancellationToken))
                return NotFound();

            return View(model);
        }
    }

    private static MatchDecisionFormModel CreateDecisionForm(Match match)
    {
        return new MatchDecisionFormModel
        {
            MatchId = match.MatchId,
            Decision = match.Status is MatchStatus.Accepted or MatchStatus.Rejected ? match.Status : null,
            Feedback = match.FeedbackMessage,
            ApplicantName = FormatUserName(match.User),
            JobTitle = match.Job?.JobTitle ?? string.Empty,
            CompanyName = match.Job?.Company?.CompanyName ?? string.Empty,
            CurrentStatus = match.Status,
            Timestamp = match.Timestamp,
        };
    }

    private async Task<bool> PopulateDecisionDisplayFieldsAsync(MatchDecisionFormModel model, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(model.MatchId, cancellationToken);
        if (match is null)
            return false;

        model.ApplicantName = FormatUserName(match.User);
        model.JobTitle = match.Job?.JobTitle ?? string.Empty;
        model.CompanyName = match.Job?.Company?.CompanyName ?? string.Empty;
        model.CurrentStatus = match.Status;
        model.Timestamp = match.Timestamp;
        return true;
    }

    private void ValidateDecisionModel(MatchDecisionFormModel model)
    {
        if (model.Decision is not (MatchStatus.Accepted or MatchStatus.Rejected))
            ModelState.AddModelError(nameof(model.Decision), "Select either Accepted or Rejected.");

        if (string.IsNullOrWhiteSpace(model.Feedback))
        {
            ModelState.AddModelError(nameof(model.Feedback), "Feedback is required.");
            return;
        }

        if (model.Feedback.Trim().Length > MatchDecisionFormModel.MaximumFeedbackLength)
            ModelState.AddModelError(nameof(model.Feedback), "Feedback must be 500 characters or fewer.");
    }

    private static bool IsDecisionValidationException(Exception exception)
    {
        return exception is ArgumentException or InvalidOperationException
            || exception is HttpRequestException { StatusCode: HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity };
    }

    private static string FormatUserName(User? user)
    {
        if (user is null)
            return string.Empty;

        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? user.Email : name;
    }
}
