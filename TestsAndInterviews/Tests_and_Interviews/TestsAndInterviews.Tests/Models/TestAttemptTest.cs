using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsAndInterviews.Tests.Models
{
    using System.Collections.Generic;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Xunit;

    /// <summary>
    /// Contains unit tests for the <see cref="TestAttempt"/> class.
    /// </summary>
    public class TestAttemptTests
    {
        private static TestAttempt MakeTestAttempt()
        {
            return new TestAttempt
            {
                Id = 1,
                TestId = 1,
                ExternalUserId = 1,
            };
        }

        [Fact]
        public void Start_WhenCalled_SetsStatusToInProgress()
        {
            // Arrange
            var attempt = MakeTestAttempt();

            // Act
            attempt.Start();

            // Assert
            Assert.Equal(TestStatus.IN_PROGRESS.ToString(), attempt.Status);
        }

        [Fact]
        public void Start_WhenCalled_SetsStartedAtToCurrentTime()
        {
            // Arrange
            var attempt = MakeTestAttempt();

            // Act
            attempt.Start();

            // Assert
            Assert.NotNull(attempt.StartedAt);
        }

        [Fact]
        public void Submit_WhenCalled_SetsStatusToCompleted()
        {
            // Arrange
            var attempt = MakeTestAttempt();

            // Act
            attempt.Submit();

            // Assert
            Assert.Equal(TestStatus.COMPLETED.ToString(), attempt.Status);
        }

        [Fact]
        public void Submit_WhenCalled_SetsCompletedAtToCurrentTime()
        {
            // Arrange
            var attempt = MakeTestAttempt();

            // Act
            attempt.Submit();

            // Assert
            Assert.NotNull(attempt.CompletedAt);
        }

        [Fact]
        public void Expire_WhenCalled_SetsStatusToExpired()
        {
            // Arrange
            var attempt = MakeTestAttempt();

            // Act
            attempt.Expire();

            // Assert
            Assert.Equal(TestStatus.EXPIRED.ToString(), attempt.Status);
        }

        [Fact]
        public void Expire_WhenCalled_SetsCompletedAtToCurrentTime()
        {
            // Arrange
            var attempt = MakeTestAttempt();

            // Act
            attempt.Expire();

            // Assert
            Assert.NotNull(attempt.CompletedAt);
        }

        [Fact]
        public void CalculateScore_WhenNoAnswers_ReturnsZero()
        {
            // Arrange
            var attempt = MakeTestAttempt();
            attempt.Test = new Test
            {
                Questions = [new Question { QuestionScore = 4f }],
            };

            // Act
            float result = attempt.CalculateScore();

            // Assert
            Assert.Equal(0f, result);
        }

        [Fact]
        public void CalculateScore_WhenNoQuestions_ReturnsZero()
        {
            // Arrange
            var attempt = MakeTestAttempt();
            attempt.Test = new Test { Questions = [] };
            attempt.Answers = [new Answer { Value = "CORRECT:4" }];

            // Act
            float result = attempt.CalculateScore();

            // Assert
            Assert.Equal(0f, result);
        }

        [Fact]
        public void CalculateScore_WhenScoreIsSet_ReturnsCorrectPercentage()
        {
            // Arrange
            var attempt = MakeTestAttempt();
            attempt.Test = new Test
            {
                Questions = [new Question { QuestionScore = 4f }],
            };
            attempt.Answers = [new Answer { Value = "CORRECT:4" }];
            attempt.Score = 4m;

            // Act
            float result = attempt.CalculateScore();

            // Assert
            Assert.Equal(100f, result);
        }
    }
}
