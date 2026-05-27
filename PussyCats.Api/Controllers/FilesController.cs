using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private const int BytesPerKilobyte = 1024;
    private const int BytesPerMegabyte = 1024 * BytesPerKilobyte;
    private const int MaxFileSizeInMb = 20;
    private const int MaxFileSize = MaxFileSizeInMb * BytesPerMegabyte;

    private readonly string uploadsPath = Path.Combine("uploads", "files");
    private readonly string legacyAvatarPath = Path.Combine("uploads", "avatars");

    public FilesController()
    {
        Directory.CreateDirectory(uploadsPath);
        Directory.CreateDirectory(legacyAvatarPath);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (fileExtension is not ".jpg" and not ".jpeg" and not ".png" and not ".pdf")
            return BadRequest("Unsupported file type.");

        if (file.Length > MaxFileSize)
            return BadRequest($"File exceeds {MaxFileSizeInMb} MB limit.");

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return Ok(new { path = fileName });
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public IActionResult GetFile(string id)
    {
        var fileName = Path.GetFileName(id);
        var fullPath = Path.Combine(uploadsPath, fileName);
        if (!System.IO.File.Exists(fullPath))
        {
            fullPath = Path.Combine(legacyAvatarPath, fileName);
        }

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var fileExtension = Path.GetExtension(id).ToLowerInvariant();
        var contentType = fileExtension switch
        {
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "image/jpeg",
        };
        return PhysicalFile(Path.GetFullPath(fullPath), contentType);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteFile(string id)
    {
        var fileName = Path.GetFileName(id);
        foreach (var root in new[] { uploadsPath, legacyAvatarPath })
        {
            var fullPath = Path.Combine(root, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                return NoContent();
            }
        }

        return NotFound();
    }
}
