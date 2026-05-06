using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Documents;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository documents;

    public DocumentsController(IDocumentRepository documents)
    {
        this.documents = documents;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var document = await documents.GetByIdAsync(id, ct);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] int userId, CancellationToken ct)
        => Ok(await documents.GetByUserIdAsync(userId, ct));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Document document, CancellationToken ct)
    {
        var saved = await documents.AddAsync(document, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.DocumentId }, saved);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        if (await documents.GetByIdAsync(id, ct) is null)
            return NotFound();
        await documents.RemoveAsync(id, ct);
        return NoContent();
    }
}
