using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository users;
    private readonly IMatchRepository matches;
    private readonly IDocumentRepository documents;

    public UsersController(IUserRepository users, IMatchRepository matches, IDocumentRepository documents)
    {
        this.users = users;
        this.matches = matches;
        this.documents = documents;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await users.GetAllAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] User user, CancellationToken ct)
    {
        user.UserId = 0;
        var saved = await users.AddAsync(user, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.UserId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] User user, CancellationToken ct)
    {
        if (await users.GetByIdAsync(id, ct) is null)
            return NotFound();
        user.UserId = id;
        await users.UpdateAsync(user, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        if (await users.GetByIdAsync(id, ct) is null)
            return NotFound();
        await users.RemoveAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id}/active")]
    public async Task<IActionResult> UpdateActive(int id, [FromBody] UpdateActiveRequest body, CancellationToken ct)
    {
        if (await users.GetByIdAsync(id, ct) is null)
            return NotFound();
        await users.UpdateActiveAccountAsync(id, body.IsActive, ct);
        await users.TouchLastUpdatedAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id}/profile-picture")]
    public async Task<IActionResult> UpdateProfilePicture(int id, [FromBody] UpdateProfilePictureRequest body, CancellationToken ct)
    {
        if (await users.GetByIdAsync(id, ct) is null)
            return NotFound();
        await users.UpdateProfilePicturePathAsync(id, body.Path ?? string.Empty, ct);
        await users.TouchLastUpdatedAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id}/profile-picture")]
    public async Task<IActionResult> RemoveProfilePicture(int id, CancellationToken ct)
    {
        if (await users.GetByIdAsync(id, ct) is null)
            return NotFound();
        await users.UpdateProfilePicturePathAsync(id, string.Empty, ct);
        await users.TouchLastUpdatedAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(int id, CancellationToken ct)
    {
        if (await users.GetByIdAsync(id, ct) is null)
            return NotFound();
        return Ok(await matches.GetByUserIdAsync(id, ct));
    }

    [HttpGet("{id}/documents")]
    public async Task<IActionResult> GetDocuments(int id, CancellationToken ct)
    {
        if (await users.GetByIdAsync(id, ct) is null)
            return NotFound();
        return Ok(await documents.GetByUserIdAsync(id, ct));
    }

    [HttpPost("{id}/cv")]
    public IActionResult ParseCv(int id) =>
        Problem("CV parsing routes through /api/files in Phase 5.", statusCode: 501);

    [HttpGet("{id}/compatibility")]
    public IActionResult GetCompatibility(int id) =>
        Problem("Compatibility computation is wired in Phase 5.", statusCode: 501);

    public record UpdateActiveRequest(bool IsActive);
    public record UpdateProfilePictureRequest(string? Path);
}
