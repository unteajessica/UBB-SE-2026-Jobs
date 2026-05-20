using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;

namespace PussyCats.Web.Controllers;

//[Authorize]
public class DocumentsController : Controller
{
    private readonly IDocumentService service;
    private readonly IWebHostEnvironment environment;

    public DocumentsController(IDocumentService service, IWebHostEnvironment environment)
    {
        this.service = service;
        this.environment = environment;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await service.GetAllAsync(ct));

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var document = await service.GetByIdAsync(id, ct);
        return document is null ? NotFound() : View(document);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string documentName, IFormFile? file, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(documentName))
        {
            ModelState.AddModelError(nameof(documentName), "Document name is required.");
            return View();
        }

        string filePath = "documents/default.txt";

        if (file != null && file.Length > 0)
        {
            try
            {
                filePath = await SaveFileAsync(file);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("file", $"Error uploading file: {ex.Message}");
                return View();
            }
        }

        var document = new Document
        {
            DocumentName = documentName,
            FilePath = filePath,
            User = new User { UserId = 0 }
        };

        await service.AddAsync(document, ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var document = await service.GetByIdAsync(id, ct);
        return document is null ? NotFound() : View(document);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string documentName, IFormFile? file, CancellationToken ct)
    {
        var document = await service.GetByIdAsync(id, ct);
        if (document is null) return NotFound();

        if (id != document.DocumentId) return BadRequest();

        if (string.IsNullOrWhiteSpace(documentName))
        {
            ModelState.AddModelError(nameof(documentName), "Document name is required.");
            return View(document);
        }

        document.DocumentName = documentName;

        if (file != null && file.Length > 0)
        {
            try
            {
                document.FilePath = await SaveFileAsync(file);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("file", $"Error uploading file: {ex.Message}");
                return View(document);
            }
        }

        await service.UpdateAsync(document, ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var document = await service.GetByIdAsync(id, ct);
        return document is null ? NotFound() : View(document);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await service.RemoveAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveFileAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine("uploads", fileName).Replace("\\", "/");
    }
}


