using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Preferences;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/preferences")]
public class PreferencesController : ControllerBase
{
    private readonly IPreferenceService preferences;

    public PreferencesController(IPreferenceService preferences)
    {
        this.preferences = preferences;
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<UserPreferences>> GetByUserId(int userId, CancellationToken cancellationToken)
    {
        var result = await preferences.GetByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Save(int userId, [FromBody] SavePreferencesRequest body, CancellationToken cancellationToken)
    {
        try
        {
            await preferences.SavePreferencesAsync(userId, body.Roles, body.WorkMode, body.Location, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(ex.Message);
        }
    }

    [HttpGet("locations")]
    public async Task<ActionResult<IReadOnlyList<string>>> SearchLocations([FromQuery] string locationsQuery, CancellationToken cancellationToken)
    {
        var matches = await preferences.SearchLocationsAsync(locationsQuery ?? string.Empty, cancellationToken);
        return Ok(matches);
    }

    public sealed record SavePreferencesRequest(
        IReadOnlyList<JobRole> Roles,
        WorkMode WorkMode,
        string Location);
}
