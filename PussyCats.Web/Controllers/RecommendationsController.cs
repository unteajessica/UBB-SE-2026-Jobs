using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Library.Services.Users;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

[Authorize] // uncomment once Dev 1 lands the auth scaffolding (Assignment 5 §A.3)
public class RecommendationsController : Controller
{
    private readonly IRecommendationService recommendations;
    private readonly IUserService users;
    private readonly IJobService jobs;

    public RecommendationsController(
        IRecommendationService recommendations,
        IUserService users,
        IJobService jobs)
    {
        this.recommendations = recommendations;
        this.users = users;
        this.jobs = jobs;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await recommendations.GetAllAsync(ct));

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var recommendation = await recommendations.GetByIdAsync(id, ct);
        return recommendation is null ? NotFound() : View(recommendation);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateDropdownsAsync(ct);
        return View(new RecommendationFormModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecommendationFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(ct);
            return View(model);
        }

        try
        {
            await recommendations.AddAsync(model.UserId, model.JobId, model.Timestamp, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            ModelState.AddModelError(string.Empty, "Selected user or job no longer exists.");
            await PopulateDropdownsAsync(ct);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var recommendation = await recommendations.GetByIdAsync(id, ct);
        if (recommendation is null) return NotFound();

        await PopulateDropdownsAsync(ct);
        return View(new RecommendationFormModel
        {
            RecommendationId = recommendation.RecommendationId,
            UserId = recommendation.User.UserId,
            JobId = recommendation.Job.JobId,
            Timestamp = recommendation.Timestamp,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RecommendationFormModel model, CancellationToken ct)
    {
        if (id != model.RecommendationId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(ct);
            return View(model);
        }

        try
        {
            await recommendations.UpdateTimestampAsync(id, model.Timestamp, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var recommendation = await recommendations.GetByIdAsync(id, ct);
        return recommendation is null ? NotFound() : View(recommendation);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await recommendations.RemoveAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(CancellationToken ct)
    {
        var allUsers = await users.GetAllAsync(ct);
        var allJobs = await jobs.GetAllAsync(ct);

        ViewBag.Users = allUsers
            .Select(user => new SelectListItem
            {
                Value = user.UserId.ToString(),
                Text = $"{user.FirstName} {user.LastName} ({user.Email})",
            })
            .ToList();

        ViewBag.Jobs = allJobs
            .Select(job => new SelectListItem
            {
                Value = job.JobId.ToString(),
                Text = job.JobTitle,
            })
            .ToList();
    }
}
