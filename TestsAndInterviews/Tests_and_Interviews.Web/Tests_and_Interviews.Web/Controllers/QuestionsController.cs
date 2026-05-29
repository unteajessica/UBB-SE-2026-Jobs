using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tests_and_Interviews.Web.Clients;
using Tests_and_Interviews.Web.Dtos;

namespace Tests_and_Interviews.Web.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly QuestionsApiClient _api;
        private readonly TestsApiClient _testsApi;

        public QuestionsController(QuestionsApiClient api, TestsApiClient testsApi)
        {
            this._api = api;
            this._testsApi = testsApi;
        }

        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Index(int? testId = null)
        {
            List<QuestionDto>? questions;
            if (testId.HasValue)
            {
                questions = await this._api.GetByTest(testId.Value);
                ViewBag.TestId = testId.Value;
                var test = await _testsApi.GetById(testId.Value);
                ViewBag.TestTitle = test?.Title;
            }
            else
            {
                questions = new List<QuestionDto>();
                ViewBag.TestId = null;
            }

            int count = (questions ?? new List<QuestionDto>()).Count;
            ViewBag.QuestionCount = count;
            ViewBag.RemainingToTarget = Math.Max(0, 25 - count);

            return View(questions);
        }

        public async Task<IActionResult> Details(int id)
        {
            QuestionDto? question = await this._api.GetQuestion(id);
            if (question == null)
                return NotFound();

            return View(question);
        }

        [Authorize(Roles = "Recruiter,Admin")]
        public IActionResult Create(int? testId = null)
        {
            ViewBag.TestId = testId;
            var model = new QuestionDto { TestId = testId };
            return View(model);
        }

        [Authorize(Roles = "Recruiter,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(QuestionDto dto, int? testId = null)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await this._api.Create(dto);

            if (testId.HasValue)
                return RedirectToAction("Index", new { testId = testId.Value });

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            QuestionDto? question = await this._api.GetQuestion(id);
            if (question == null)
                return NotFound();

            return View(question);
        }

        [Authorize(Roles = "Recruiter,Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, QuestionDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await this._api.Update(id, dto);

            if (dto.TestId.HasValue)
                return RedirectToAction("Index", new { testId = dto.TestId.Value });

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            QuestionDto? question = await this._api.GetQuestion(id);
            if (question == null)
                return NotFound();

            return View(question);
        }

        [Authorize(Roles = "Recruiter,Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            QuestionDto? question = await this._api.GetQuestion(id);
            int? testId = question?.TestId;

            await this._api.Delete(id);

            if (testId.HasValue)
                return RedirectToAction("Index", new { testId = testId.Value });

            return RedirectToAction("Index");
        }
    }
}
