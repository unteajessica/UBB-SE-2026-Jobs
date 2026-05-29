namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// IGradingService interface provides methods to grade different types of questions and calculate the final score for a test attempt.
    /// </summary>
    public interface IGradingService
    {
        /// <summary>
        /// Grades a single choice question by comparing the provided answer with the correct answer.
        /// If the answer is correct, it updates the answer value to indicate correctness and the score awarded.
        /// </summary>
        /// <param name="question">The question being graded.</param>
        /// <param name="answer">The answer provided by the user.</param>
        void GradeSingleChoice(Question question, Answer answer);

        /// <summary>
        /// Grades a multiple choice question by comparing the list of selected answers with the list of correct answers.
        /// </summary>
        /// <param name="question"> The question being graded.</param>
        /// <param name="answer"> The answer provided by the user, which should be a comma-separated list of selected option indices.</param>
        void GradeMultipleChoice(Question question, Answer answer);

        /// <summary>
        /// Grades a text question by comparing the provided answer with the correct answer, ignoring case and leading/trailing whitespace.
        /// </summary>
        /// <param name="question">The question being graded.</param>
        /// <param name="answer">The answer provided by the user.</param>
        void GradeText(Question question, Answer answer);

        /// <summary>
        /// Grades a true/false question by comparing the provided answer with the correct answer, ignoring case and leading/trailing whitespace.
        /// </summary>
        /// <param name="question">The question being graded.</param>
        /// <param name="answer">The answer provided by the user, which should be "true" or "false".</param>
        void GradeTrueFalse(Question question, Answer answer);

        /// <summary>
        /// Calculates the final score for a test attempt by summing up the scores from all answers that are marked as correct.
        /// </summary>
        /// <param name="attempt">The test attempt for which the final score is being calculated.</param>
        /// <returns>The total score calculated for the test attempt.</returns>
        float CalculateFinalScore(TestAttempt attempt);
    }
}
