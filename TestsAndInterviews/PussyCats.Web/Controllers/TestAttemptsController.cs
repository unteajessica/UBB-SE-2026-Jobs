using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Web.Clients;
using PussyCats.Web.Dtos;

namespace PussyCats.Web.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class TestAttemptsController : Controller
    {
        private readonly TestAttemptsApiClient _attemptsApi;
        private readonly AnswersApiClient _answersApi;
        private readonly QuestionsApiClient _questionsApi;

        public TestAttemptsController(TestAttemptsApiClient attemptsApi, AnswersApiClient answersApi, QuestionsApiClient questionsApi)
        {
            _attemptsApi = attemptsApi;
            _answersApi = answersApi;
            _questionsApi = questionsApi;
        }

        // Start a new attempt then redirect to Details which shows answers
        [HttpGet]
        public async Task<IActionResult> Start(int testId)
        {
            // Determine current user id from claims
            var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameId) || !int.TryParse(nameId, out var userId))
            {
                return Unauthorized();
            }

            // Create a new attempt for the current user
            var dto = new TestAttemptDto { TestId = testId, ExternalUserId = userId, Status = "IN_PROGRESS", StartedAt = DateTime.UtcNow };
            try
            {
                await _attemptsApi.Create(dto);
            }
            catch (HttpRequestException ex)
            {
                // log and return a friendly error
                // In a real app use an ILogger; for now return BadRequest with details
                return Problem(detail: ex.Message, statusCode: 502);
            }

            // Find the attempt we just created for this user and test
            var attempt = await _attemptsApi.FindByUserAndTest(userId, testId);
            if (attempt == null)
            {
                // no attempt found after creating one - return informative response
                return Problem(detail: "Attempt created but then could not be found.", statusCode: 500);
            }

            return RedirectToAction("Details", new { id = attempt.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var attempt = await _attemptsApi.GetById(id);
            if (attempt == null) return NotFound();

            // load answers for this attempt
            var answers = await _answersApi.GetAnswersByAttempt(id);

            // load questions for the test so candidate can answer if no answers exist
            var questions = new List<QuestionDto>();
            try
            {
                if (attempt.TestId != 0)
                {
                    questions = await _questionsApi.GetByTest(attempt.TestId) ?? new List<QuestionDto>();
                }
            }
            catch
            {
                // ignore errors fetching questions; we will still render existing answers
            }

            // Prepare answer placeholders for each question if none submitted yet
            var answerList = answers ?? new List<AnswerDto>();
            if ((answerList == null || !answerList.Any()) && questions.Any())
            {
                answerList = questions.Select(q => new AnswerDto
                {
                    AttemptId = attempt.Id,
                    QuestionId = q.Id,
                    Question = new QuestionDto { Id = q.Id, QuestionText = q.QuestionText }
                }).ToList();
            }

            attempt.Answers = answerList ?? new List<AnswerDto>();

            return View(attempt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAnswers(int attemptId, List<AnswerDto> answers)
        {
            if (answers == null || !answers.Any())
                return RedirectToAction("Details", new { id = attemptId });

            try
            {
                foreach (var a in answers)
                {
                    // ensure AttemptId is set
                    a.AttemptId = attemptId;
                    await _answersApi.SaveAnswer(a);
                }
            }
            catch (HttpRequestException ex)
            {
                return Problem(detail: ex.Message, statusCode: 502);
            }

            return RedirectToAction("Details", new { id = attemptId });
        }
    }
}

