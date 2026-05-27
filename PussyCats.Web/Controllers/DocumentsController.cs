using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Users;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly IDocumentService documents;
    private readonly IUserService users;

    public DocumentsController(IDocumentService documents, IUserService users)
    {
        this.documents = documents;
        this.users = users;
    }

    public async Task<IActionResult> Index(int? userId, CancellationToken cancellationToken)
    {
        var allUsers = await users.GetAllAsync(cancellationToken);
        ViewBag.Users = allUsers.Select(user => new SelectListItem
        {
            Value = user.UserId.ToString(),
            Text = $"{user.FirstName} {user.LastName} ({user.Email})",
            Selected = user.UserId == userId
        }).ToList();

        var selectedUserId = userId ?? allUsers.FirstOrDefault()?.UserId ?? 0;
        ViewBag.SelectedUserId = selectedUserId;

        var userDocuments = selectedUserId > 0
            ? await documents.GetDocumentsByUserIdAsync(selectedUserId, cancellationToken)
            : new List<Document>();

        return View(userDocuments);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : View(document);
    }

    public async Task<IActionResult> Create(int? userId, CancellationToken cancellationToken)
    {
        await PopulateUsersDropdownAsync(cancellationToken);
        return View(new DocumentFormModel { UserId = userId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentFormModel model, CancellationToken cancellationToken)
    {
        if (model.File is null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "Please upload a file.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateUsersDropdownAsync(cancellationToken);
            return View(model);
        }

        try
        {
            await using var fileStream = model.File!.OpenReadStream();
            await documents.UploadDocumentFromStreamAsync(
                model.UserId,
                model.DocumentName,
                model.File.FileName,
                model.File.ContentType,
                fileStream,
                false,
                cancellationToken);

            return RedirectToAction(nameof(Index), new { userId = model.UserId });
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            ModelState.AddModelError(nameof(model.UserId), "Selected user does not exist.");
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(nameof(model.File), "The selected file could not be uploaded or parsed.");
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.File), exception.Message);
        }

        await PopulateUsersDropdownAsync(cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        if (document?.User is null)
        {
            return NotFound();
        }

        await PopulateUsersDropdownAsync(cancellationToken);
        return View(new DocumentFormModel
        {
            DocumentId = document.DocumentId,
            UserId = document.User.UserId,
            DocumentName = document.DocumentName,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentFormModel model, CancellationToken cancellationToken)
    {
        if (id != model.DocumentId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateUsersDropdownAsync(cancellationToken);
            return View(model);
        }

        var document = await documents.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        document.DocumentName = model.DocumentName;
        await documents.UpdateAsync(document, cancellationToken);
        return RedirectToAction(nameof(Index), new { userId = model.UserId });
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        return document?.User is null ? NotFound() : View(document);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, int userId, CancellationToken cancellationToken)
    {
        await documents.RemoveAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { userId });
    }

    private async Task PopulateUsersDropdownAsync(CancellationToken cancellationToken)
    {
        var allUsers = await users.GetAllAsync(cancellationToken);
        ViewBag.Users = allUsers.Select(user => new SelectListItem
        {
            Value = user.UserId.ToString(),
            Text = $"{user.FirstName} {user.LastName} ({user.Email})",
        }).ToList();
    }
}
