namespace PussyCats.Web.Controllers
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using PussyCats.Web.Clients;
    using PussyCats.Web.Dtos;
    using PussyCats.Web.Models;

    /// <summary>
    /// Controller responsible for managing test-related web views, including 
    /// test administration for recruiters and the test-taking experience for candidates.
    /// </summary>
    public class TestsController : Controller
    {
        private readonly TestsApiClient _api;
        private readonly LeaderboardApiClient _leaderboardApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsController"/> class.
        /// </summary>
        /// <param name="api">The API client used to communicate with the backend test services.</param>
        /// <param name="leaderboardApi">The API client used to communicate with the backend leaderboard services.</param>
        public TestsController(TestsApiClient api, LeaderboardApiClient leaderboardApi)
        {
            this._api = api;
            this._leaderboardApi = leaderboardApi;
        }

        /// <summary>
        /// Displays the dashboard containing all available tests grouped by category.
        /// </summary>
        /// <returns>An asynchronous task returning the Index view with the TestsViewModel.</returns>
        public async Task<IActionResult> Index()
        {
            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : -1;

            List<string> categories = await this._api.GetCategories();
            TestsViewModel viewModel = new TestsViewModel();

            foreach (string category in categories)
            {
                List<TestDto> tests = await this._api.GetByCategory(category);

                foreach (TestDto test in tests)
                {
                    bool hasFinished = false;

                    if (userId != -1 && User.IsInRole("Candidate"))
                    {
                        var existingAttempt = await this._api.GetAttemptByUserAndTestAsync(userId, test.Id);

                        if (existingAttempt != null)
                        {
                            var savedAnswers = await this._api.GetAnswersByAttemptIdAsync(existingAttempt.Id);

                            if (savedAnswers != null && savedAnswers.Any())
                            {
                                hasFinished = true;
                            }
                        }
                    }

                    viewModel.Tests.Add(new TestCardViewModel
                    {
                        TestId = test.Id,
                        Title = test.Title,
                        Category = test.Category,
                        QuestionTypeLabel = test.QuestionTypeLabel,
                        HasBeenTaken = hasFinished
                    });
                }
            }

            return View(viewModel);
        }
        /// <summary>
        /// Retrieves and displays the details of a specific test.
        /// </summary>
        /// <param name="id">The unique identifier of the test.</param>
        /// <returns>An asynchronous task returning the Details view.</returns>
        public async Task<IActionResult> Details(int id)
        {
            TestDto? test = await this._api.GetById(id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        /// <summary>
        /// Initiates the test-taking process for a candidate by redirecting them to the Take action.
        /// </summary>
        /// <param name="id">The unique identifier of the test to start.</param>
        /// <returns>A redirection to the Take action.</returns>
        [Authorize(Roles = "Candidate")]
        public IActionResult Start(int id)
        {
            // Redirect to the Take action which already implements the test-taking flow
            return RedirectToAction("Take", new { id });
        }

        /// <summary>
        /// Displays the form for creating a new test (Admin/Recruiter only).
        /// </summary>
        /// <returns>The Create view.</returns>
        [Authorize(Roles = "Recruiter,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Submits the newly created test to the backend API.
        /// </summary>
        /// <param name="dto">The data transfer object containing the new test details.</param>
        /// <returns>An asynchronous task returning a redirection to the Index if successful.</returns>
        [Authorize(Roles = "Recruiter,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(TestDto dto, bool? manageQuestions)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var created = await this._api.Create(dto);

            if (manageQuestions.GetValueOrDefault(false) && created != null)
            {
                return RedirectToAction("Index", "Questions", new { testId = created.Id });
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Displays the form for editing an existing test (Admin/Recruiter only).
        /// </summary>
        /// <param name="id">The unique identifier of the test to edit.</param>
        /// <returns>An asynchronous task returning the Edit view.</returns>
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            TestDto? test = await this._api.GetById(id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        /// <summary>
        /// Submits the updated test details to the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the test being updated.</param>
        /// <param name="dto">The data transfer object containing the updated test details.</param>
        /// <returns>An asynchronous task returning a redirection to the Index if successful.</returns>
        [Authorize(Roles = "Recruiter,Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, TestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            await this._api.Update(id, dto);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Displays the confirmation page for deleting a test (Admin/Recruiter only).
        /// </summary>
        /// <param name="id">The unique identifier of the test to delete.</param>
        /// <returns>An asynchronous task returning the Delete confirmation view.</returns>
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            TestDto? test = await this._api.GetById(id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        /// <summary>
        /// Processes the deletion of a test by communicating with the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the test to delete.</param>
        /// <returns>An asynchronous task returning a redirection to the Index.</returns>
        [Authorize(Roles = "Recruiter,Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await this._api.Delete(id);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> Take(int id)
        {
            TestDto? test = await this._api.GetById(id);
            if (test == null) return NotFound();

            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : 1;

            var existingAttempt = await this._api.GetAttemptByUserAndTestAsync(userId, id);

            if (existingAttempt != null)
            {
                var savedAnswers = await this._api.GetAnswersByAttemptIdAsync(existingAttempt.Id);
                if (savedAnswers.Any())
                {
                    ViewBag.AttemptId = existingAttempt.Id;
                    return View("AlreadyTaken");
                }
            }
            else
            {
                await this._api.StartAttemptAsync(userId, id);
            }

            List<QuestionDto> questions = await this._api.GetQuestionsByTestIdAsync(id);
            TakeTestViewModel viewModel = new TakeTestViewModel
            {
                TestId = test.Id,
                Title = test.Title,
                Questions = questions
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Candidate")]
        [HttpPost]
        public async Task<IActionResult> Submit(TakeTestViewModel model)
        {
            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdClaim, out int parsedId) ? parsedId : 1;

            List<QuestionDto> questions = await this._api.GetQuestionsByTestIdAsync(model.TestId);

            int validAnswerCount = 0;
            if (model.Answers != null)
            {
                foreach (var kvp in model.Answers)
                {
                    if (kvp.Value != null && kvp.Value.Any(v => !string.IsNullOrWhiteSpace(v)))
                    {
                        validAnswerCount++;
                    }
                }
            }

            if (!ModelState.IsValid || validAnswerCount < questions.Count)
            {
                model.Questions = questions;
                ModelState.AddModelError(string.Empty, "You must provide an answer for every question before submitting.");
                return View("Take", model);
            }

            var history = await this._api.GetValidAttemptsByTestIdAsync(model.TestId);
            bool hasFinished = history.Any(a => a.ExternalUserId == userId &&
                (a.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) || a.CompletedAt.HasValue));

            if (hasFinished)
            {
                return View("AlreadyTaken");
            }

            var attempt = await this._api.GetAttemptByUserAndTestAsync(userId, model.TestId);
            if (attempt == null)
            {
                return NotFound("Active test attempt not found.");
            }

            float maxPossibleScore = 0f;
            List<AnswerDto> gradedAnswers = new List<AnswerDto>();

            foreach (var kvp in model.Answers)
            {
                var q = questions.FirstOrDefault(x => x.Id == kvp.Key);
                if (q == null) continue;

                string joinedAnswer = string.Join(",", kvp.Value.Where(v => !string.IsNullOrWhiteSpace(v)));
                if (string.IsNullOrEmpty(joinedAnswer)) continue;

                var gradeRequest = new
                {
                    Question = new
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionTypeString = q.QuestionType,
                        QuestionScore = q.QuestionScore,
                        QuestionAnswer = q.QuestionAnswer
                    },
                    Answer = new
                    {
                        QuestionId = q.Id,
                        AttemptId = attempt.Id,
                        Value = joinedAnswer
                    }
                };

                AnswerDto gradedAnswer = await this._api.GradeAnswerAsync(q.QuestionType, gradeRequest);
                await this._api.SaveAnswerAsync(gradedAnswer);
                gradedAnswers.Add(gradedAnswer);
            }

            foreach (var q in questions)
            {
                maxPossibleScore += q.QuestionScore;
            }

            var scorePayload = new
            {
                Id = attempt.Id,
                Answers = gradedAnswers.Select(a => new { Value = a.Value }).ToList()
            };

            float rawScore = await this._api.CalculateFinalScoreAsync(scorePayload);

            attempt.Status = "COMPLETED";
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.Score = (decimal)rawScore;

            if (maxPossibleScore > 0)
            {
                attempt.PercentageScore = (decimal)((rawScore / maxPossibleScore) * 100f);
            }

            await this._api.UpdateAttemptAsync(attempt.Id, attempt);

            // Recalculate the leaderboard for this test now that the attempt is completed
            await this._leaderboardApi.RecalculateLeaderboardAsync(model.TestId);

            return RedirectToAction("Result", new { score = rawScore, maxScore = maxPossibleScore });
        }

        /// <summary>
        /// Displays the final calculated score to the candidate after a successful test submission.
        /// </summary>
        /// <param name="score">The final percentage score returned by the grading API.</param>
        /// <returns>The Result view displaying the final score.</returns>
        [Authorize(Roles = "Candidate")]
        public IActionResult Result(float score, float maxScore)
        {
            ViewBag.MaxScore = maxScore;
            return View(score);
        }
    }
}
