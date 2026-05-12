using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository documents;
    private readonly IUserRepository users;

    public DocumentsController(IDocumentRepository documents, IUserRepository users)
    {
        this.documents = documents;
        this.users = users;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] int userId, CancellationToken cancellationToken)
        => Ok(await documents.GetByUserIdAsync(userId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] DocumentAddRequest body, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(body.UserId, cancellationToken);
        if (user is null)
            return NotFound($"User {body.UserId} not found.");

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await documents.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await documents.RemoveAsync(id, cancellationToken);
        return NoContent();
    }
}
