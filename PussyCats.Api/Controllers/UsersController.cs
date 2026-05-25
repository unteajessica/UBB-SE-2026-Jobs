using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services;
using PussyCats.Library.Services.CvParsing;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService users;
    private readonly IMatchService matches;
    private readonly IDocumentService documents;
    private readonly IUserProfileService userProfileService;

    public UsersController(IUserService users, IMatchService matches, IDocumentService documents, IUserProfileService userProfileService)
    {
        this.users = users;
        this.matches = matches;
        this.documents = documents;
        this.userProfileService = userProfileService;
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
    public async Task<IActionResult> ParseCv(
    int id,
    IFormFile file,
    [FromServices] ICvParsingService cvParsingService,
    CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension != ".json")
        {
            return BadRequest("Only .json files are supported.");
        }

        using var reader = new StreamReader(file.OpenReadStream());

        var content = await reader.ReadToEndAsync(cancellationToken);

        var parsedUser = cvParsingService.ParseCvFile(content, extension);

        return Ok(parsedUser);
    }

    [HttpGet("{id}/compatibility")]
    public IActionResult GetCompatibility(int id) =>

        Problem("Compatibility computation is wired in Phase 5.", statusCode: 501);

    [HttpGet("{id}/experience")]
    public async Task<IActionResult> RecalculateExperience(int id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound();

        int experiencePoints = await userProfileService.RecalculateLevelAsync(user, cancellationToken);
        return Ok(new { TotalExperiencePoints = experiencePoints });
    }

    [HttpGet("{id}/skill-tests")]
    public async Task<IActionResult> GetSkillTests(int id, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        return Ok(await userProfileService.GetSkillTestsForUserAsync(id, cancellationToken));
    }

    [HttpGet("{id}/is-active")]
    public async Task<IActionResult> IsProfileAvailable(int id, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        return Ok(await userProfileService.IsProfileAvailableAsync(id, cancellationToken));
    }
    
    // This might not be needed, but I'll leave it here just in case.
    [HttpGet("{id}/parsed-cv")]
    public async Task<IActionResult> GetParsedCv(int id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound();
        string parsedCv = Helpers.GenerateParsedCvText(user);
        return Ok(new { ParsedCv = parsedCv });
    }

    public record UpdateActiveRequest(bool IsActive);
    public record UpdateProfilePictureRequest(string? Path);
}
