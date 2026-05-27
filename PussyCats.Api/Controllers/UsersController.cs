using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services;
using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.CvParsing;
using PussyCats.Library.Services.PdfExport;
using PussyCats.Library.Services.SkillGapService;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService users;
    private readonly IMatchService matches;
    private readonly IDocumentService documents;
    private readonly IUserProfileService userProfileService;
    private readonly ICvParsingService cvParsingService;
    private readonly ICompletenessService completenessService;
    private readonly ISkillGapService skillGapService;
    private readonly IPdfExportService pdfExportService;

    public UsersController(
        IUserService users,
        IMatchService matches,
        IDocumentService documents,
        IUserProfileService userProfileService,
        ICvParsingService cvParsingService,
        ICompletenessService completenessService,
        ISkillGapService skillGapService,
        IPdfExportService pdfExportService)
    {
        this.users = users;
        this.matches = matches;
        this.documents = documents;
        this.userProfileService = userProfileService;
        this.cvParsingService = cvParsingService;
        this.completenessService = completenessService;
        this.skillGapService = skillGapService;
        this.pdfExportService = pdfExportService;
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

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(email, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("exists-by-email/{email}")]
    public async Task<IActionResult> ExistsByEmail(string email, CancellationToken cancellationToken)
    {
        return Ok(await users.ExistsWithEmailAsync(email, cancellationToken));
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

    [HttpPut("{id}/profile")]
    public async Task<IActionResult> SaveProfile(int id, [FromBody] User user, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await userProfileService.SaveAsync(id, user, cancellationToken);
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
    public async Task<IActionResult> UploadCv(
    int id,
    IFormFile file,
    CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest(new { detail = "No file uploaded." });

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var fileType = Path.GetExtension(file.FileName);

            var parsedUser = cvParsingService.ParseCvFile(content, fileType);

            await userProfileService.SaveAsync(id, parsedUser, cancellationToken);

            return Ok(parsedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    [HttpGet("{id}/compatibility")]
    public IActionResult GetCompatibility(int id) =>

        Problem("Compatibility computation is wired in Phase 5.", statusCode: 501);

    [HttpGet("{id}/completeness")]
    public async Task<IActionResult> GetCompleteness(int id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound();

        return Ok(new
        {
            Percentage = completenessService.CalculateCompleteness(user),
            NextPrompt = completenessService.GetNextEmptyFieldPrompt(user),
        });
    }

    [HttpGet("{id}/skill-gap")]
    public async Task<IActionResult> GetSkillGap(int id, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();

        return Ok(new
        {
            Summary = await skillGapService.GetSummaryAsync(id, cancellationToken),
            MissingSkills = await skillGapService.GetMissingSkillsAsync(id, cancellationToken),
            UnderscoredSkills = await skillGapService.GetUnderscoredSkillsAsync(id, cancellationToken),
        });
    }

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

    [HttpGet("{id}/cv/html")]
    public async Task<IActionResult> GetCvHtml(int id, CancellationToken cancellationToken)
    {
        var user = await userProfileService.GetProfileAsync(id, cancellationToken);
        if (user is null)
            return NotFound();

        var html = await pdfExportService.RenderHtmlAsync(user);
        return Content(html, "text/html");
    }

    [HttpGet("{id}/cv/pdf")]
    public async Task<IActionResult> DownloadCvPdf(int id, CancellationToken cancellationToken)
    {
        var user = await userProfileService.GetProfileAsync(id, cancellationToken);
        if (user is null)
            return NotFound();

        var pdf = await pdfExportService.GeneratePdfAsync(user);
        return File(pdf, "application/pdf", $"{user.FirstName}_{user.LastName}_CV.pdf");
    }

    public record UpdateActiveRequest(bool IsActive);
    public record UpdateProfilePictureRequest(string? Path);
}
