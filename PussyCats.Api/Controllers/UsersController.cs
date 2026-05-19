using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService users;
    private readonly IMatchService matches;
    private readonly IDocumentService documents;

    public UsersController(IUserService users, IMatchService matches, IDocumentService documents)
    {
        this.users = users;
        this.matches = matches;
        this.documents = documents;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await users.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] User user, CancellationToken cancellationToken)
    {
        user.UserId = 0;
        var saved = await users.AddAsync(user, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.UserId }, saved);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] User user, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        user.UserId = id;
        await users.UpdateAsync(user, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await users.RemoveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/active")]
    public async Task<IActionResult> UpdateActive(int id, [FromBody] UpdateActiveRequest body, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await users.SetActiveAsync(id, body.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/profile-picture")]
    public async Task<IActionResult> UpdateProfilePicture(int id, [FromBody] UpdateProfilePictureRequest body, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await users.SetProfilePicturePathAsync(id, body.Path ?? string.Empty, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/profile-picture")]
    public async Task<IActionResult> RemoveProfilePicture(int id, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await users.SetProfilePicturePathAsync(id, string.Empty, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(int id, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        return Ok(await matches.GetMatchesForUserAsync(id, cancellationToken));
    }

    [HttpGet("{id}/documents")]
    public async Task<IActionResult> GetDocuments(int id, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        return Ok(await documents.GetDocumentsByUserIdAsync(id, cancellationToken));
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
