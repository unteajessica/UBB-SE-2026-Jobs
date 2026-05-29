
namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Controller used for grading operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GradingController : ControllerBase
    {
        private readonly IGradingService gradingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GradingController"/> class.
        /// </summary>
        /// <param name="gradingService">Injected grading service.</param>
        public GradingController(IGradingService gradingService)
        {
            this.gradingService = gradingService;
        }

        /// <summary>
        /// Grades a single choice question.
        /// </summary>
        [HttpPost("single-choice")]
        public IActionResult GradeSingleChoice([FromBody] GradeRequest request)
        {
            this.gradingService.GradeSingleChoice(request.Question, request.Answer);
            return this.Ok(request.Answer);
        }

        /// <summary>
        /// Grades a multiple choice question.
        /// </summary>
        [HttpPost("multiple-choice")]
        public IActionResult GradeMultipleChoice([FromBody] GradeRequest request)
        {
            this.gradingService.GradeMultipleChoice(request.Question, request.Answer);
            return this.Ok(request.Answer);
        }

        /// <summary>
        /// Grades a text question.
        /// </summary>
        [HttpPost("text")]
        public IActionResult GradeText([FromBody] GradeRequest request)
        {
            this.gradingService.GradeText(request.Question, request.Answer);
            return this.Ok(request.Answer);
        }

        /// <summary>
        /// Grades a true/false question.
        /// </summary>
        [HttpPost("true-false")]
        public IActionResult GradeTrueFalse([FromBody] GradeRequest request)
        {
            this.gradingService.GradeTrueFalse(request.Question, request.Answer);
            return this.Ok(request.Answer);
        }

        /// <summary>
        /// Calculates the final score.
        /// </summary>
        [HttpPost("final-score")]
        public ActionResult<float> CalculateFinalScore([FromBody] TestAttempt attempt)
        {
            var result = this.gradingService.CalculateFinalScore(attempt);
            return this.Ok(result);
        }
    }

    /// <summary>
    /// DTO used for grading requests.
    /// </summary>
    public class GradeRequest
    {
        /// <summary>
        /// Gets or sets question.
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// Gets or sets answer.
        /// </summary>
        public Answer Answer { get; set; }
    }
}