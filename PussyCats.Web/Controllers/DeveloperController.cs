using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Developers;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

public class DeveloperController : Controller
{
    private readonly IDeveloperService developer;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public DeveloperController(IDeveloperService developer)
    {
        this.developer = developer;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var posts = await developer.GetPostsAsync(cancellationToken);
        var interactions = await developer.GetInteractionsAsync(cancellationToken);
        var model = BuildFeed(posts, interactions, CurrentUserId);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeveloperPostCreateModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var posts = await developer.GetPostsAsync(cancellationToken);
            var interactions = await developer.GetInteractionsAsync(cancellationToken);
            ViewBag.CreateModel = model;
            return View("Index", BuildFeed(posts, interactions, CurrentUserId));
        }

        try
        {
            await developer.AddPostAsync(CurrentUserId, model.ParameterType, model.Value, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var posts = await developer.GetPostsAsync(cancellationToken);
            var interactions = await developer.GetInteractionsAsync(cancellationToken);
            ViewBag.CreateModel = model;
            return View("Index", BuildFeed(posts, interactions, CurrentUserId));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Interact(int postId, string action, CancellationToken cancellationToken)
    {
        var interactionType = action == "like" ? DeveloperInteractionType.Like : DeveloperInteractionType.Dislike;
        var interactions = await developer.GetInteractionsAsync(cancellationToken);
        var existing = interactions.FirstOrDefault(i =>
            i.Developer.DeveloperId == CurrentUserId &&
            i.DeveloperPost.DeveloperPostId == postId);

        if (existing is not null && existing.Type == interactionType)
            await developer.RemoveInteractionAsync(existing.DeveloperInteractionId, cancellationToken);
        else
            await developer.AddInteractionAsync(CurrentUserId, postId, interactionType, cancellationToken);

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
                .Where(i => i.DeveloperPost.DeveloperPostId == post.DeveloperPostId)
                .ToList();
            return new DeveloperPostViewModel(post, postInteractions, currentUserId);
        }).ToList();
    }
}
