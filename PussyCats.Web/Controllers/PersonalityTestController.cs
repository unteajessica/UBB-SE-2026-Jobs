using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.PersonalityTestService;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

public class PersonalityTestController : Controller
{
    private readonly IPersonalityTestService service;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public PersonalityTestController(IPersonalityTestService service)
    {
        this.service = service;
    }

    // GET: /PersonalityTest
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await service.GetByUserIdAsync(CurrentUserId, cancellationToken);
        if (result is null)
            return RedirectToAction(nameof(Take));

        return View(result);
    }

    // GET: /PersonalityTest/Take
    public IActionResult Take()
    {
        var questions = PersonalityTestService.LoadQuestions();
        return View(questions);
    }

    // POST: /PersonalityTest/Submit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(PersonalityTestSubmitModel submitModel)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Take));

        var questions = PersonalityTestService.LoadQuestions()
            .ToDictionary(question => question.SortOrder.ToString());

        var answers = new Dictionary<Question, AnswerValue>();
        foreach (var (key, answerValue) in submitModel.Answers)
        {
            if (!questions.TryGetValue(key, out var question))
                continue;

            answers[question] = (AnswerValue)answerValue;
        }

        var traitScores = service.CalculateTraitScores(answers);
        var roleScores = service.CalculateRoleScores(traitScores);
        var topRoles = service.GetTopRoles(roleScores, count: 3);

        var model = new SelectRoleModel
        {
            Answers = submitModel.Answers,
            TopRoles = topRoles.Select(role => new RoleOption
            {
                Role = role.Key,
                DisplayName = role.Key.ToString(),
                Score = role.Value,
            }).ToList(),
        };

        return View("SelectRole", model);
    }

    // POST: /PersonalityTest/SaveResult
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(PersonalityTestSubmitModel submitModel, JobRole selectedRole, CancellationToken cancellationToken)
    {
        var questions = PersonalityTestService.LoadQuestions()
            .ToDictionary(question => question.SortOrder.ToString());

        var answers = new Dictionary<Question, AnswerValue>();
        foreach (var (key, answerValue) in submitModel.Answers)
        {
            if (!questions.TryGetValue(key, out var question))
                continue;

            answers[question] = (AnswerValue)answerValue;
        }

        await service.SaveResultAsync(CurrentUserId, answers, selectedRole, cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}