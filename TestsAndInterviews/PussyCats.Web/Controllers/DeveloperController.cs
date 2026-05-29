using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Developers;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

[Authorize]
public class DeveloperController : Controller
{
    private readonly IDeveloperService developerService;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public DeveloperController(IDeveloperService developerService)
    {
        this.developerService = developerService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var posts = await developerService.GetPostsAsync(cancellationToken);
        var interactions = await developerService.GetInteractionsAsync(cancellationToken);
        var model = BuildFeed(posts, interactions, CurrentUserId);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeveloperPostCreateModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var posts = await developerService.GetPostsAsync(cancellationToken);
            var interactions = await developerService.GetInteractionsAsync(cancellationToken);
            ViewBag.CreateModel = model;
            return View("Index", BuildFeed(posts, interactions, CurrentUserId));
        }

        try
        {
            await developerService.AddPostAsync(CurrentUserId, model.ParameterType, model.Value, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            var posts = await developerService.GetPostsAsync(cancellationToken);
            var interactions = await developerService.GetInteractionsAsync(cancellationToken);
            ViewBag.CreateModel = model;
            return View("Index", BuildFeed(posts, interactions, CurrentUserId));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Interact(int postId, string action, CancellationToken cancellationToken)
    {
        var interactionType = action == "like" ? DeveloperInteractionType.Like : DeveloperInteractionType.Dislike;
        var interactions = await developerService.GetInteractionsAsync(cancellationToken);
        var existing = interactions.FirstOrDefault(developerInteraction =>
            developerInteraction.Developer.DeveloperId == CurrentUserId &&
            developerInteraction.DeveloperPost.DeveloperPostId == postId);

        if (existing is not null && existing.Type == interactionType)
            await developerService.RemoveInteractionAsync(existing.DeveloperInteractionId, cancellationToken);
        else
            await developerService.AddInteractionAsync(CurrentUserId, postId, interactionType, cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private static IReadOnlyList<DeveloperPostViewModel> BuildFeed(
        IReadOnlyList<DeveloperPost> posts,
        IReadOnlyList<DeveloperInteraction> interactions,
        int currentUserId)
    {
        return posts.Select(post =>
        {
            var postInteractions = interactions
                .Where(developerInteraction => developerInteraction.DeveloperPost.DeveloperPostId == post.DeveloperPostId)
                .ToList();
            return new DeveloperPostViewModel(post, postInteractions, currentUserId);
        }).ToList();
    }
}
