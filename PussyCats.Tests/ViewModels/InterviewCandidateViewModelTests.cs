using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services.Interfaces;
using Tests_and_Interviews.ViewModels;
using Xunit;

namespace PussyCats.Tests.ViewModels
{
    public class InterviewCandidateViewModelTests
    {
        [Fact]
        public async Task InitializeAsync_WhenSessionExists_UpdatesStartTimeAndLoadsQuestions()
        {
            var sessionId = 10;
            var positionId = 5;

            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotificationService = new Mock<INotificationService>();

            var fakeSession = new InterviewSession { Id = sessionId, PositionId = positionId };
            var fakeQuestions = new List<Question> { new Question(), new Question() };

            mockSessionService
                .Setup(s => s.StartSessionAsync(sessionId))
                .ReturnsAsync((fakeSession, fakeQuestions));

            var sut = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotificationService.Object);

            await sut.LoadData(sessionId);

            mockSessionService.Verify(
                s => s.StartSessionAsync(sessionId),
                Times.Once);
        }

        [Fact]
        public async Task NextQuestion_IncrementsIndex_AndUpdatesQuestionText()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            var session = new InterviewSession { PositionId = 101 };

            var questions = new List<Question>
            {
                new Question { QuestionText = "What is C#?" },
                new Question { QuestionText = "What is MVVM?" },
            };

            mockSessionService
                .Setup(s => s.StartSessionAsync(1))
                .ReturnsAsync((session, questions));

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            await vm.LoadData(1);

            vm.StartQuestions();

            Assert.Equal("What is C#?", vm.QuestionText);

            vm.NextQuestionCommand.Execute(null);

            Assert.Equal("What is MVVM?", vm.QuestionText);
        }

        [Fact]
        public async Task CompletingQuestions_ShowsCompletionMessage()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            var questions = new List<Question>
            {
                new Question { QuestionText = "Q1" },
                new Question { QuestionText = "Q2" },
            };

            mockSessionService
                .Setup(s => s.StartSessionAsync(It.IsAny<int>()))
                .ReturnsAsync((new InterviewSession { PositionId = 20 }, questions));

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            await vm.LoadData(1);

            vm.StartQuestions();
            vm.NextQuestionCommand.Execute(null);
            vm.NextQuestionCommand.Execute(null);

            Assert.Contains("Congratulation", vm.QuestionText);
        }

        [Fact]
        public async Task ResetQuestionsTest()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            var questions = new List<Question>
            {
                new Question { QuestionText = "Q1" },
                new Question { QuestionText = "Q2" },
            };

            mockSessionService
                .Setup(s => s.StartSessionAsync(It.IsAny<int>()))
                .ReturnsAsync((new InterviewSession { PositionId = 30 }, questions));

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            await vm.LoadData(1);

            vm.StartQuestions();
            vm.NextQuestionCommand.Execute(null);

            Assert.Equal("Q2", vm.QuestionText);

            vm.ResetQuestions();

            Assert.Equal("Questions will start after starting recording", vm.QuestionText);

            vm.NextQuestionCommand.Execute(null);

            Assert.Equal("Q1", vm.QuestionText);
        }

        [Fact]
        public async Task LoadData_WhenServiceThrows_ShowsGenericErrorMessage()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            mockSessionService
                .Setup(s => s.StartSessionAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            await vm.LoadData(1);

            Assert.Equal("An error occurred while loading the session.", vm.QuestionText);
        }

        [Fact]
        public async Task SubmitRecordingSuccessfully()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            var session = new InterviewSession { Id = 1, PositionId = 101 };

            var questions = new List<Question>
            {
                new Question { QuestionText = "What is C#?" },
                new Question { QuestionText = "What is MVVM?" },
            };

            mockSessionService
                .Setup(s => s.StartSessionAsync(1))
                .ReturnsAsync((session, questions));

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            await vm.LoadData(1);

            vm.RecordingFilePath = "path/to/video.mp4";
            vm.StartQuestions();
            vm.NextQuestionCommand.Execute(null);
            vm.NextQuestionCommand.Execute(null);
            vm.SubmitRecordingCommand.Execute(null);

            await Task.Delay(100);

            mockSessionService.Verify(
                s => s.SubmitRecordingAsync(session, "path/to/video.mp4"),
                Times.Once);

            mockNotification.Verify(
                n => n.ShowSimpleNotification(
                    "Video uploaded",
                    "Your interview video was uploaded successfully."),
                Times.Once);
        }

        [Fact]
        public async Task FailedToShowNotificationTest()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            var session = new InterviewSession { Id = 1, PositionId = 101 };

            mockSessionService
                .Setup(s => s.StartSessionAsync(1))
                .ReturnsAsync((session, new List<Question>()));

            mockNotification
                .Setup(n => n.ShowSimpleNotification(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Notification system failed"));

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            await vm.LoadData(1);

            vm.StartQuestions();
            vm.RecordingFilePath = "path/to/video.mp4";
            vm.SubmitRecordingCommand.Execute(null);

            await Task.Delay(100);

            Assert.Equal("Video uploaded, but failed to show notification.", vm.QuestionText);
        }

        [Fact]
        public void SubmitRecording_WhenSessionNull()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            vm.SubmitRecordingCommand.Execute(null);

            Assert.Equal("No session loaded. Cannot submit recording.", vm.QuestionText);
        }

        [Fact]
        public async Task SubmitRecordingWhenServiceThrowsError()
        {
            var mockSessionService = new Mock<IInterviewSessionService>();
            var mockNotification = new Mock<INotificationService>();

            var session = new InterviewSession { Id = 1, PositionId = 101 };

            mockSessionService
                .Setup(s => s.StartSessionAsync(1))
                .ReturnsAsync((session, new List<Question>()));

            mockSessionService
                .Setup(s => s.SubmitRecordingAsync(It.IsAny<InterviewSession>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database update failed"));

            var vm = new InterviewCandidateViewModel(
                mockSessionService.Object,
                mockNotification.Object);

            await vm.LoadData(1);

            vm.StartQuestions();
            vm.RecordingFilePath = "path/to/video.mp4";
            vm.SubmitRecordingCommand.Execute(null);

            await Task.Delay(100);

            Assert.Equal("Failed to upload video. Please try again.", vm.QuestionText);
        }
    }
}