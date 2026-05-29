// <copyright file="GradingService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Tests_and_Interviews_API.Helpers;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// GradingService class provides methods to grade different types of questions and calculate the final score for a test attempt.
    /// </summary>
    public class GradingService : IGradingService
    {
        /// <summary>
        /// Grades a single choice question by comparing the provided answer with the correct answer.
        /// If the answer is correct, it updates the answer value to indicate correctness and the score awarded.
        /// </summary>
        /// <param name="question">The question being graded.</param>
        /// <param name="answer">The answer provided by the user.</param>
        public void GradeSingleChoice(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }

            if (answer.Value.Trim() == question.QuestionAnswer.Trim())
            {
                answer.Value = $"CORRECT:{question.QuestionScore}";
            }
        }

        /// <summary>
        /// Grades a multiple choice question by comparing the list of selected answers with the list of correct answers.
        /// </summary>
        /// <param name="question"> The question being graded.</param>
        /// <param name="answer"> The answer provided by the user, which should be a comma-separated list of selected option indices.</param>
        public void GradeMultipleChoice(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }

            var correctIndexes = new List<int>();
            var selectedIndexes = new List<int>();

            foreach (var part in question.QuestionAnswer.Trim().TrimStart('[').TrimEnd(']').Split(','))
            {
                if (int.TryParse(part.Trim(), out int parsedIndex))
                {
                    correctIndexes.Add(parsedIndex);
                }
            }

            foreach (var part in answer.Value.Trim().TrimStart('[').TrimEnd(']').Split(','))
            {
                if (int.TryParse(part.Trim(), out int parsedIndex))
                {
                    selectedIndexes.Add(parsedIndex);
                }
            }

            int numberOfCorrectAnswers = correctIndexes.Count;
            int numberOfWrongAnswers = selectedIndexes.Count(index => !correctIndexes.Contains(index));
            int numberOfWrongOptions = TestConstants.OptionsPerQuestion - numberOfCorrectAnswers;

            float scorePerCorrect = numberOfCorrectAnswers > 0
                ? question.QuestionScore / numberOfCorrectAnswers
                : 0f;

            float penaltyPerWrong = numberOfWrongOptions > 0
                ? question.QuestionScore / numberOfWrongOptions
                : 0f;

            float score = 0f;
            foreach (var index in selectedIndexes)
            {
                if (correctIndexes.Contains(index))
                {
                    score += scorePerCorrect;
                }
                else
                {
                    score -= penaltyPerWrong;
                }
            }

            if (score < 0f)
            {
                score = 0f;
            }

            answer.Value = $"PARTIAL:{score}";
        }

        /// <summary>
        /// Grades a text question by comparing the provided answer with the correct answer, ignoring case and leading/trailing whitespace.
        /// </summary>
        /// <param name="question">The question being graded.</param>
        /// <param name="answer">The answer provided by the user.</param>
        public void GradeText(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }

            bool isCorrect = string.Equals(
                answer.Value.Trim(),
                question.QuestionAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                answer.Value = $"CORRECT:{question.QuestionScore}";
            }
        }

        /// <summary>
        /// Grades a true/false question by comparing the provided answer with the correct answer, ignoring case and leading/trailing whitespace.
        /// </summary>
        /// <param name="question">The question being graded.</param>
        /// <param name="answer">The answer provided by the user, which should be "true" or "false".</param>
        public void GradeTrueFalse(Question question, Answer answer)
        {
            if (question.QuestionAnswer == null)
            {
                return;
            }

            bool isCorrect = string.Equals(
                answer.Value.Trim(),
                question.QuestionAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                answer.Value = $"CORRECT:{question.QuestionScore}";
            }
        }

        /// <summary>
        /// Calculates the final score for a test attempt by summing up the scores from all answers that are marked as correct.
        /// </summary>
        /// <param name="attempt">The test attempt for which the final score is being calculated.</param>
        /// <returns>The total score calculated for the test attempt.</returns>
        public float CalculateFinalScore(TestAttempt attempt)
        {
            float totalScore = 0f;

            foreach (var answer in attempt.Answers)
            {
                if (answer.Value.StartsWith("CORRECT:", StringComparison.OrdinalIgnoreCase))
                {
                    string scorePart = answer.Value.Substring("CORRECT:".Length);
                    if (float.TryParse(scorePart, NumberStyles.Float, CultureInfo.InvariantCulture, out float points))
                    {
                        totalScore += points;
                    }
                }
                else if (answer.Value.StartsWith("PARTIAL:", StringComparison.OrdinalIgnoreCase))
                {
                    string scorePart = answer.Value.Substring("PARTIAL:".Length);
                    if (float.TryParse(scorePart, NumberStyles.Float, CultureInfo.InvariantCulture, out float points))
                    {
                        totalScore += points;
                    }
                }
            }

            attempt.Score = (decimal)totalScore;

            return totalScore;
        }
    }
}