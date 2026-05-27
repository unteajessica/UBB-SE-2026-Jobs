using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.ImageStorage;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.Users;
using System.Security.Claims;

namespace PussyCats.Web.Controllers;

[Authorize]
public class UserProfileController : Controller
{
    private readonly IUserProfileService userProfileService;
    private readonly ICompletenessService completenessService;
    private readonly IImageStorageService imageStorage;
    private readonly IUserService userService;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public UserProfileController(
        IUserProfileService userProfileService,
        ICompletenessService completenessService,
        IImageStorageService imageStorage,
        IUserService userService)
    {
        this.userProfileService = userProfileService;
        this.completenessService = completenessService;
        this.imageStorage = imageStorage;
        this.userService = userService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await userProfileService.GetProfileAsync(CurrentUserId, cancellationToken);
        if (user is null)
            return NotFound();

        ViewBag.CompletenessPercentage = completenessService.CalculateCompleteness(user);
        ViewBag.NextFieldPrompt = completenessService.GetNextEmptyFieldPrompt(user);
        int totalXp = await userProfileService.RecalculateLevelAsync(user, cancellationToken);
        ViewBag.TotalXp = totalXp;
        ViewBag.LevelProgressPercent = CalculateLevelProgress(totalXp, user.CurrentLevel);
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(CancellationToken cancellationToken)
    {
        var user = await userProfileService.GetProfileAsync(CurrentUserId, cancellationToken);
        if (user is null)
            return NotFound();

        await userProfileService.UpdateAccountStatusAsync(CurrentUserId, !user.ActiveAccount, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar, CancellationToken cancellationToken)
    {
        if (avatar is null || avatar.Length == 0)
        {
            TempData["Error"] = "Please select an image file.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await using var stream = avatar.OpenReadStream();
            imageStorage.CheckFileSize(stream);
            stream.Position = 0;
            var path = await imageStorage.SaveImageAsync(stream, avatar.FileName, cancellationToken);
            await userService.SetProfilePicturePathAsync(CurrentUserId, path, cancellationToken);
        }
        catch (Exception exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private static int CalculateLevelProgress(int xp, int level)
    {
        var thresholds = new[] { 0, 100, 250, 500, 800 };
        int start = level >= 1 && level <= thresholds.Length ? thresholds[level - 1] : 0;
        int end = level < thresholds.Length ? thresholds[level] : thresholds[^1];
        if (end <= start) return 100;
        return (int)Math.Clamp((xp - start) * 100.0 / (end - start), 0, 100);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAvatar(CancellationToken cancellationToken)
    {
        var user = await userProfileService.GetProfileAsync(CurrentUserId, cancellationToken);
        if (user is not null && !string.IsNullOrWhiteSpace(user.ProfilePicturePath))
            await imageStorage.DeleteImageAsync(user.ProfilePicturePath, cancellationToken);

        await userService.SetProfilePicturePathAsync(CurrentUserId, string.Empty, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await userProfileService.GetProfileAsync(CurrentUserId, CancellationToken.None);

        if (user == null)
        {
            user = new User();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await userProfileService.SaveAsync(CurrentUserId, model, CancellationToken.None);

            TempData["Success"] = "Profile saved successfully.";

            return RedirectToAction(nameof(Edit));
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            return View(model);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCv(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { detail = "No file uploaded." });

        try
        {
            await using var stream = file.OpenReadStream();
            var user = await userProfileService.UploadCvAsync(CurrentUserId, stream, file.FileName, cancellationToken);
            return Json(user);
        }
        catch (Exception exception)
        {
            return BadRequest(new { detail = exception.Message });
        }
    }
}
