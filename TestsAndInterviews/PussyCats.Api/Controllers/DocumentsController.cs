using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Users;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService documents;
    private readonly ILocalDocumentFileService documentFiles;
    private readonly IUserService users;

    public DocumentsController(
        IDocumentService documents,
        ILocalDocumentFileService documentFiles,
        IUserService users)
    {
        this.documents = documents;
        this.documentFiles = documentFiles;
        this.users = users;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, CancellationToken cancellationToken)
    {
        if (userId.HasValue)
        {
            return Ok(await documents.GetDocumentsByUserIdAsync(userId.Value, cancellationToken));
        }
        return Ok(await documents.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] DocumentAddRequest body, CancellationToken cancellationToken)
    {
        const int mockUserId = 1;
        int resolvedUserId = body.UserId > 0 ? body.UserId : mockUserId;

        var user = await users.GetByIdAsync(resolvedUserId, cancellationToken);
        if (user is null)
            return NotFound($"User {resolvedUserId} not found.");

        var document = new Document
        {
            User = user,
            DocumentName = body.DocumentName,
            FilePath = body.FilePath,
        };
        document.DocumentId = 0;
        var saved = await documents.AddAsync(document, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.DocumentId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DocumentUpdateRequest body, CancellationToken cancellationToken)
    {
        var existing = await documents.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound();

        existing.DocumentName = body.DocumentName;
        existing.FilePath = body.FilePath;

        await documents.UpdateAsync(existing, cancellationToken);
        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadRequest body, CancellationToken cancellationToken)
    {
        if (body.File is null || body.File.Length == 0)
        {
            return BadRequest("Please upload a file.");
        }

        try
        {
            await using var stream = body.File.OpenReadStream();
            var saved = await documents.UploadDocumentFromStreamAsync(
                body.UserId,
                body.DocumentName,
                body.File.FileName,
                body.File.ContentType,
                stream,
                body.IsCv,
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = saved.DocumentId }, saved);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await documents.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await documentFiles.DeleteDocumentAsync(id, cancellationToken);
        return NoContent();
    }

    public sealed class DocumentUploadRequest
    {
        public int UserId { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public bool IsCv { get; set; }

        public IFormFile? File { get; set; }
    }

    [HttpGet("{id}/url")]
    public async Task<IActionResult> GetUrl(int id, CancellationToken cancellationToken)
    {
        try
        {
            var url = await documentFiles
                .GetDocumentUrlAsync(id, cancellationToken);

            return Ok(url);
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(exception.Message);
        }
    }
}
