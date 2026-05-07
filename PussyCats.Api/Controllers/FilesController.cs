using Microsoft.AspNetCore.Mvc;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly string uploadsPath = Path.Combine("uploads", "avatars");

    public FilesController()
    {
        Directory.CreateDirectory(uploadsPath);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not ".jpg" and not ".jpeg" and not ".png")
            return BadRequest("Unsupported file type.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File exceeds 5MB limit.");

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream, ct);

        return Ok(new { path = fileName });
    }

    [HttpGet("{id}")]
    public IActionResult GetFile(string id)
    {
        var fullPath = Path.Combine(uploadsPath, Path.GetFileName(id));
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var ext = Path.GetExtension(id).ToLowerInvariant();
        var contentType = ext is ".png" ? "image/png" : "image/jpeg";
        return PhysicalFile(Path.GetFullPath(fullPath), contentType);
    }
}
