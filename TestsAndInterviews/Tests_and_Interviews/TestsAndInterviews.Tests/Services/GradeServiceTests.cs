// <copyright file="GradingServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestsAndInterviews.Tests.Services
{
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services;
    using Xunit;

    /// <summary>
    /// Contains unit tests for the <see cref="GradingService"/> class.
    /// </summary>
    public class GradingServiceTests
    {
        private static GradingService MakeGradingService()
        {
            // GradingService handles pure logic and does not rely on repositories or HttpClient,
            // therefore it can be instantiated directly.
            return new GradingService();
        }

        private static Question MakeQuestion(QuestionType type, string? correctAnswer, float score = 4f)
        {
            return new Question
            {
                QuestionTypeString = type.ToString(),
                QuestionAnswer = correctAnswer,
                QuestionScore = score,
            };
        }

        private static Answer MakeAnswer(string? value)
        {
            return new Answer { Value = value };
        }

        // GradeSingleChoice tests

        [Fact]
        public void GradeSingleChoice_WhenAnswerIsCorrect_SetsCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE, "2");
            var answer = MakeAnswer("2");

            // Act
            gradingService.GradeSingleChoice(question, answer);

            // Assert
            Assert.StartsWith("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeSingleChoice_WhenAnswerIsCorrect_SetsFullScore()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE, "2");
            var answer = MakeAnswer("2");

            // Act
            gradingService.GradeSingleChoice(question, answer);

            // Assert
            Assert.Equal($"CORRECT:{question.QuestionScore}", answer.Value);
        }

        [Fact]
        public void GradeSingleChoice_WhenAnswerIsWrong_DoesNotSetCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE, "2");
            var answer = MakeAnswer("3");

            // Act
            gradingService.GradeSingleChoice(question, answer);

            // Assert
            Assert.DoesNotContain("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeSingleChoice_WhenAnswerIsWrong_PreservesOriginalValue()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE, "2");
            var answer = MakeAnswer("3");

            // Act
            gradingService.GradeSingleChoice(question, answer);

            // Assert
            Assert.Equal("3", answer.Value);
        }

        [Fact]
        public void GradeSingleChoice_WhenCorrectAnswerIsNull_DoesNotModifyAnswer()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE, null);
            var answer = MakeAnswer("2");

            // Act
            gradingService.GradeSingleChoice(question, answer);

            // Assert
            Assert.Equal("2", answer.Value);
        }

        [Fact]
        public void GradeSingleChoice_WhenAnswerHasWhitespace_StillGradesCorrectly()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.SINGLE_CHOICE, " 2 ");
            var answer = MakeAnswer(" 2 ");

            // Act
            gradingService.GradeSingleChoice(question, answer);

            // Assert
            Assert.StartsWith("CORRECT:", answer.Value!);
        }

        // GradeTrueFalse tests

        [Fact]
        public void GradeTrueFalse_WhenAnswerIsTrue_AndCorrectAnswerIsTrue_SetsCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TRUE_FALSE, "true");
            var answer = MakeAnswer("true");

            // Act
            gradingService.GradeTrueFalse(question, answer);

            // Assert
            Assert.StartsWith("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeTrueFalse_WhenAnswerIsFalse_AndCorrectAnswerIsTrue_DoesNotSetCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TRUE_FALSE, "true");
            var answer = MakeAnswer("false");

            // Act
            gradingService.GradeTrueFalse(question, answer);

            // Assert
            Assert.DoesNotContain("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeTrueFalse_WhenAnswerIsCaseInsensitive_SetsCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TRUE_FALSE, "true");
            var answer = MakeAnswer("TRUE");

            // Act
            gradingService.GradeTrueFalse(question, answer);

            // Assert
            Assert.StartsWith("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeTrueFalse_WhenCorrectAnswerIsNull_DoesNotModifyAnswer()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TRUE_FALSE, null);
            var answer = MakeAnswer("true");

            // Act
            gradingService.GradeTrueFalse(question, answer);

            // Assert
            Assert.Equal("true", answer.Value);
        }

        // GradeText tests

        [Fact]
        public void GradeText_WhenAnswerMatchesExactly_SetsCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TEXT, "polymorphism");
            var answer = MakeAnswer("polymorphism");

            // Act
            gradingService.GradeText(question, answer);

            // Assert
            Assert.StartsWith("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeText_WhenAnswerIsCaseInsensitive_SetsCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TEXT, "polymorphism");
            var answer = MakeAnswer("POLYMORPHISM");

            // Act
            gradingService.GradeText(question, answer);

            // Assert
            Assert.StartsWith("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeText_WhenAnswerIsWrong_DoesNotSetCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TEXT, "polymorphism");
            var answer = MakeAnswer("inheritance");

            // Act
            gradingService.GradeText(question, answer);

            // Assert
            Assert.DoesNotContain("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeText_WhenAnswerHasExtraWhitespace_SetsCorrectPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TEXT, "polymorphism");
            var answer = MakeAnswer("  polymorphism  ");

            // Act
            gradingService.GradeText(question, answer);

            // Assert
            Assert.StartsWith("CORRECT:", answer.Value!);
        }

        [Fact]
        public void GradeText_WhenCorrectAnswerIsNull_DoesNotModifyAnswer()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.TEXT, null);
            var answer = MakeAnswer("polymorphism");

            // Act
            gradingService.GradeText(question, answer);

            // Assert
            Assert.Equal("polymorphism", answer.Value);
        }

        // GradeMultipleChoice tests

        [Fact]
        public void GradeMultipleChoice_WhenAllCorrectAnswersSelected_SetsPartialPrefix()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE, "[0,1]");
            var answer = MakeAnswer("[0,1]");

            // Act
            gradingService.GradeMultipleChoice(question, answer);

            // Assert
            Assert.StartsWith("PARTIAL:", answer.Value!);
        }

        [Fact]
        public void GradeMultipleChoice_WhenAllCorrectAnswersSelected_SetsFullScore()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE, "[0,1]", 4f);
            var answer = MakeAnswer("[0,1]");

            // Act
            gradingService.GradeMultipleChoice(question, answer);

            // Assert
            Assert.Equal("PARTIAL:4", answer.Value);
        }

        [Fact]
        public void GradeMultipleChoice_WhenNoAnswerSelected_SetsZeroScore()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE, "[0,1]");
            var answer = MakeAnswer("[]");

            // Act
            gradingService.GradeMultipleChoice(question, answer);

            // Assert
            Assert.Equal("PARTIAL:0", answer.Value);
        }

        [Fact]
        public void GradeMultipleChoice_WhenOnlyWrongAnswersSelected_SetsZeroScore()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE, "[0,1]");
            var answer = MakeAnswer("[2,3]");

            // Act
            gradingService.GradeMultipleChoice(question, answer);

            // Assert
            Assert.Equal("PARTIAL:0", answer.Value);
        }

        [Fact]
        public void GradeMultipleChoice_WhenCorrectAnswerIsNull_DoesNotModifyAnswer()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE, null);
            var answer = MakeAnswer("[0,1]");

            // Act
            gradingService.GradeMultipleChoice(question, answer);

            // Assert
            Assert.Equal("[0,1]", answer.Value);
        }

        // CalculateFinalScore tests

        [Fact]
        public void CalculateFinalScore_WhenNoAnswers_ReturnsZero()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var attempt = new TestAttempt { Answers = [] };

            // Act
            float result = gradingService.CalculateFinalScore(attempt);

            // Assert
            Assert.Equal(0f, result);
        }

        [Fact]
        public void CalculateFinalScore_WhenAllAnswersCorrect_ReturnsSumOfScores()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var attempt = new TestAttempt
            {
                Answers =
                [
                    new Answer { Value = "CORRECT:4" },
                    new Answer { Value = "CORRECT:4" },
                ],
            };

            // Act
            float result = gradingService.CalculateFinalScore(attempt);

            // Assert
            Assert.Equal(8f, result);
        }

        [Fact]
        public void CalculateFinalScore_WhenAnswersArePartial_IncludesPartialScores()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var attempt = new TestAttempt
            {
                Answers =
                [
                    new Answer { Value = "PARTIAL:2" },
                    new Answer { Value = "CORRECT:4" },
                ],
            };

            // Act
            float result = gradingService.CalculateFinalScore(attempt);

            // Assert
            Assert.Equal(6f, result);
        }

        [Fact]
        public void CalculateFinalScore_WhenAllAnswersWrong_ReturnsZero()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var attempt = new TestAttempt
            {
                Answers =
                [
                    new Answer { Value = "wrong answer" },
                    new Answer { Value = "another wrong" },
                ],
            };

            // Act
            float result = gradingService.CalculateFinalScore(attempt);

            // Assert
            Assert.Equal(0f, result);
        }

        [Fact]
        public void CalculateFinalScore_WhenCalled_UpdatesAttemptScore()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var attempt = new TestAttempt
            {
                Answers = [new Answer { Value = "CORRECT:4" }],
            };

            // Act
            gradingService.CalculateFinalScore(attempt);

            // Assert
            Assert.Equal(4m, attempt.Score);
        }

        [Fact]
        public void GradeMultipleChoice_WhenNoCorrectAnswersDefined_SetsZeroScore()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE, "[]");
            var answer = MakeAnswer("[0]");

            // Act
            gradingService.GradeMultipleChoice(question, answer);

            // Assert
            Assert.Equal("PARTIAL:0", answer.Value);
        }

        [Fact]
        public void GradeMultipleChoice_WhenAllOptionsAreCorrect_SetsFullScore()
        {
            // Arrange
            var gradingService = MakeGradingService();
            var question = MakeQuestion(QuestionType.MULTIPLE_CHOICE, "[0,1,2,3,4,5]");
            var answer = MakeAnswer("[0,1,2,3,4,5]");

            // Act
            gradingService.GradeMultipleChoice(question, answer);

            // Assert
            Assert.StartsWith("PARTIAL:", answer.Value!);
        }
    }
}