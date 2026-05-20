using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.PersonalityTestService;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

//[Authorize]
public class PersonalityTestController : Controller
{
    private readonly IPersonalityTestService service;

    public PersonalityTestController(IPersonalityTestService service)
    {
        this.service = service;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        //replace!!!
        //var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await service.GetByUserIdAsync(1, ct);
        return View(result);
    }

    public IActionResult Take()
    {
        var questions = PersonalityTestService.LoadQuestions();
        return View(questions);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Dictionary<int, int> answers, CancellationToken cancellationToken)
    {
        var questions = PersonalityTestService.LoadQuestions();

        var answersDict = questions
            .Where(question => answers.ContainsKey(question.SortOrder))
            .ToDictionary(question => question, question => (AnswerValue)answers[question.SortOrder]);
        //replace also here userdId with auth
        var top = await service.CalculateAsync(1, answersDict, cancellationToken);

        var model = new SelectRoleModel
        {
            TopRoles = top.Select(r => new RoleOption
            {
                Role = r.Key,
                /*DisplayName = System.Text.RegularExpressions.Regex
                    .Replace(r.Key.ToString(), "(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " "),*/
                DisplayName = r.Key.ToString(),
                Score = r.Value
            }).ToList(),
            Answers = answers
        };

        return View("SelectRole", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(SelectRoleModel model, CancellationToken cancellationToken)
    {
        //replace after auth
        var userId = 1; 
        var questions = PersonalityTestService.LoadQuestions();

        var answersDict = questions
            .Where(q => model.Answers.ContainsKey(q.SortOrder))
            .ToDictionary(q => q, q => (AnswerValue)model.Answers[q.SortOrder]);

        await service.SaveResultAsync(userId, answersDict, model.SelectedRole, cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}