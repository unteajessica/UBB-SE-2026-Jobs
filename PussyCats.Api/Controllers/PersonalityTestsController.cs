
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.PersonalityTestService;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/personality-tests")]
public class PersonalityTestsController : ControllerBase
{
    private readonly IPersonalityTestService service;

    public PersonalityTestsController(IPersonalityTestService service)
    {
        this.service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] int userId, CancellationToken cancellationToken)
    {
        var result = await service.GetByUserIdAsync(userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    public record SavePersonalityTestRequest(int UserId, JobRole SelectedRole, List<PersonalityTestAnswer> Answers);

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SavePersonalityTestRequest request, CancellationToken cancellationToken)
    {
        var questions = PersonalityTestService.LoadQuestions();
        var answersDict = request.Answers
            .Select(a => new
            {
                Question = questions.FirstOrDefault(q => q.SortOrder == a.SortOrder),
                Answer = (AnswerValue)a.Answer,
            })
            .Where(x => x.Question is not null)
            .ToDictionary(x => x.Question!, x => x.Answer);

        await service.SaveResultAsync(request.UserId, answersDict, request.SelectedRole, cancellationToken);
        return Ok();
    }
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] SavePersonalityTestRequest request)
    {
        var questions = PersonalityTestService.LoadQuestions();

        var answersDict = request.Answers
            .Select(a => new
            {
                Question = questions.FirstOrDefault(q => q.SortOrder == a.SortOrder),
                Answer = (AnswerValue)a.Answer,
            })
            .Where(x => x.Question is not null)
            .ToDictionary(x => x.Question!, x => x.Answer);

        var traits = service.CalculateTraitScores(answersDict);
        var roles = service.CalculateRoleScores(traits);
        var top = service.GetTopRoles(roles, 3);

        return Ok(top);
    }
}