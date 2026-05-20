using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService documents;
    private readonly IUserService users;

    public DocumentsController(IDocumentService documents, IUserService users)
    {
        this.documents = documents;
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await documents.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await documents.RemoveAsync(id, cancellationToken);
        return NoContent();
    }
}
