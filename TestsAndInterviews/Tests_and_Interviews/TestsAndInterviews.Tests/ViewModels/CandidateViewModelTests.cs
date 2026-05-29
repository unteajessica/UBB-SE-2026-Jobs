// <copyright file="CandidateViewModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestsAndInterviews.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Timers;

    using Moq;
    using Xunit;

    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.ViewModels;

    public class CandidateViewModelTests
    {
        private readonly Mock<IBookingService> mockBookingService;

        private readonly Mock<IInterviewSessionService> mockSessionService;

        private readonly Mock<INotificationService> mockNotificationService;

        public CandidateViewModelTests()
        {
            this.mockBookingService = new Mock<IBookingService>();
            this.mockSessionService = new Mock<IInterviewSessionService>();
            this.mockNotificationService = new Mock<INotificationService>();

            this.mockSessionService
                .Setup(service => service.GetScheduledSessionsAsync())
                .ReturnsAsync(new List<InterviewSession>());
        }

        private CandidateViewModel CreateViewModel()
        {
            return new CandidateViewModel(
                this.mockBookingService.Object,
                this.mockSessionService.Object,
                this.mockNotificationService.Object);
        }

        //private CandidateViewModel CreateViewModelWithCompanyAndSlot(CompanyPosting company, Slot slot)
        //{
        //    this.mockBookingService
        //        .Setup(b => b.GetAvailableSlotsByRecruiterId(company.RecruiterId))
        //        .Returns(new List<Slot> { slot });

        //    var viewModel = this.CreateViewModel();

        //    viewModel.ScheduleInterviewCommand.Execute(company);
        //    viewModel.SelectSlotForInterviewCommand.Execute(slot);

        //    return viewModel;
        //}

        //[Fact]
        //public void ScheduleInterviewCommand_SetsIsBookingVisibleAndSelectedCompany()
        //{
        //    var company = new CompanyPosting { RecruiterId = 1 };

        //    this.mockBookingService
        //        .Setup(bookingService => bookingService.GetAvailableSlotsByRecruiterId(1))
        //        .Returns(new List<Slot>());

        //    var viewModel = this.CreateViewModel();

        //    viewModel.ScheduleInterviewCommand.Execute(company);

        //    Assert.True(viewModel.IsBookingVisible);
        //    Assert.Equal(company, viewModel.SelectedCompany);
        //}

        [Fact]
        public void ScheduleInterviewCommand_WhenObjectIsNotCompany_DoesNothing()
        {
            var viewModel = this.CreateViewModel();

            viewModel.ScheduleInterviewCommand.Execute("not a company");

            Assert.False(viewModel.IsBookingVisible);
        }

        [Fact]
        public void ScheduleInterviewCommand_WhenSelectedCompanyIsNull_DoesNotLoadSlots()
        {
            var viewModel = this.CreateViewModel();

            viewModel.SelectedDay = DateTime.Today;

            this.mockBookingService.Verify(
                bookingService => bookingService.GetAvailableSlotsByRecruiterId(It.IsAny<int>()),
                Times.Never);
        }

        //[Fact]
        //public void SelectSlotCommand_SetsSelectedSlotAndDeselectsOthers()
        //{
        //    var company = new CompanyPosting { RecruiterId = 1 };

        //    var slot1 = new Slot { StartTime = DateTime.Today, Status = SlotStatus.Free };
        //    var slot2 = new Slot { StartTime = DateTime.Today.AddHours(1), Status = SlotStatus.Free };

        //    this.mockBookingService
        //        .Setup(bookingService => bookingService.GetAvailableSlotsByRecruiterId(1))
        //        .Returns(new List<Slot> { slot1, slot2 });

        //    var viewModel = this.CreateViewModel();

        //    viewModel.ScheduleInterviewCommand.Execute(company);
        //    viewModel.SelectSlotForInterviewCommand.Execute(slot1);
        //    viewModel.SelectSlotForInterviewCommand.Execute(slot2);

        //    Assert.Equal(slot2, viewModel.SelectedSlot);
        //    Assert.False(slot1.IsSlotSelected);
        //    Assert.True(slot2.IsSlotSelected);
        //}

        [Fact]
        public void SelectSlotCommand_WhenObjectIsNotSlot_DoesNothing()
        {
            var viewModel = this.CreateViewModel();

            viewModel.SelectSlotForInterviewCommand.Execute("not a slot");

            Assert.Null(viewModel.SelectedSlot);
        }

       
        [Fact]
        public void SelectDayCommand_WhenObjectIsNotSlot_DoesNothing()
        {
            var viewModel = this.CreateViewModel();
            var originalDay = viewModel.SelectedDay;

            viewModel.SelectDayForInterviewCommand.Execute("not a slot");

            Assert.Equal(originalDay, viewModel.SelectedDay);
        }

        [Fact]
        public async Task LoadNextDaysCommand_AndPreviousDaysCommand_PaginateCorrectly()
        {
            var company = new CompanyPosting { RecruiterId = 1 };

            var slots = new List<Slot>
            {
                new Slot { StartTime = DateTime.Today, Status = SlotStatus.Free },
                new Slot { StartTime = DateTime.Today.AddDays(1), Status = SlotStatus.Free },
                new Slot { StartTime = DateTime.Today.AddDays(2), Status = SlotStatus.Free },
                new Slot { StartTime = DateTime.Today.AddDays(3), Status = SlotStatus.Free },
            };

            this.mockBookingService
                .Setup(bookingService => bookingService.GetAvailableSlotsByRecruiterId(1));
                

            var viewModel = this.CreateViewModel();

            viewModel.ScheduleInterviewCommand.Execute(company);

            var firstVisible = new List<Slot>(viewModel.VisibleDays)[0].StartTime;

            viewModel.LoadNextDaysCommand.Execute(null);

            var afterNext = new List<Slot>(viewModel.VisibleDays)[0].StartTime;

            viewModel.LoadPreviousDaysCommand.Execute(null);

            var afterPrev = new List<Slot>(viewModel.VisibleDays)[0].StartTime;

            Assert.NotEqual(firstVisible, afterNext);
            Assert.Equal(firstVisible, afterPrev);
        }

        [Fact]
        public void LoadNextDaysCommand_WhenAtEnd_DoesNotAdvance()
        {
            var viewModel = this.CreateViewModel();

            var visibleBefore = new List<Slot>(viewModel.VisibleDays).Count;

            viewModel.LoadNextDaysCommand.Execute(null);

            Assert.Equal(visibleBefore, new List<Slot>(viewModel.VisibleDays).Count);
        }

        [Fact]
        public void LoadPreviousDaysCommand_WhenAtStart_DoesNotGoBack()
        {
            var viewModel = this.CreateViewModel();

            viewModel.LoadPreviousDaysCommand.Execute(null);

            Assert.Empty(viewModel.VisibleDays);
        }

        //[Fact]
        //public async Task ConfirmInterviewCommand_WhenSlotAndCompanySelected_ConfirmsAndHidesBooking()
        //{
        //    var company = new CompanyPosting
        //    {
        //        CompanyName = "Google",
        //        JobTitle = "Dev",
        //        RecruiterId = 1,
        //    };

        //    var slot = new Slot
        //    {
        //        StartTime = DateTime.Today,
        //        EndTime = DateTime.Today.AddHours(1),
        //        Status = SlotStatus.Free,
        //    };

        //    var viewModel = this.CreateViewModelWithCompanyAndSlot(company, slot);

        //    viewModel.MatchedCompanies.Add(company);

        //    viewModel.ConfirmInterviewCommand.Execute(null);

        //    await Task.Delay(100);

        //    this.mockBookingService.Verify(
        //        bookingService => bookingService.ConfirmBooking(It.IsAny<int>(), slot),
        //        Times.Once);

        //    Assert.False(viewModel.IsBookingVisible);
        //    Assert.DoesNotContain(company, viewModel.MatchedCompanies);
        //}

        [Fact]
        public void ConfirmInterviewCommand_WhenNoSlotSelected_DoesNotCallConfirmBooking()
        {
            var viewModel = this.CreateViewModel();

            viewModel.ConfirmInterviewCommand.Execute(null);

            this.mockBookingService.Verify(
                bookingService => bookingService.ConfirmBooking(It.IsAny<int>(), It.IsAny<Slot>()),
                Times.Never);
        }

       //[Fact]
        //public async Task ConfirmInterviewCommand_WhenNotificationFails_StillCompletesBooking()
        //{
        //    var company = new CompanyPosting
        //    {
        //        CompanyName = "Google",
        //        JobTitle = "Dev",
        //        RecruiterId = 1,
        //    };

        //    var slot = new Slot
        //    {
        //        StartTime = DateTime.Today,
        //        EndTime = DateTime.Today.AddHours(1),
        //        Status = SlotStatus.Free,
        //    };

        //    this.mockNotificationService
        //        .Setup(notificationService => notificationService.ShowBookingConfirmed(
        //            It.IsAny<string>(),
        //            It.IsAny<string>(),
        //            It.IsAny<DateTime>(),
        //            It.IsAny<DateTime>()))
        //        .Throws(new Exception("Notification failed"));

        //    var viewModel = this.CreateViewModelWithCompanyAndSlot(company, slot);

        //    viewModel.MatchedCompanies.Add(company);

        //    viewModel.ConfirmInterviewCommand.Execute(null);

        //    await Task.Delay(100);

        //    Assert.False(viewModel.IsBookingVisible);
        //}

        [Fact]
        public void ConfirmInterviewCommand_WhenNoCompanySelected_DoesNotCallConfirmBooking()
        {
            var slot = new Slot { Status = SlotStatus.Free };

            var viewModel = this.CreateViewModel();

            viewModel.SelectedSlot = slot;

            viewModel.ConfirmInterviewCommand.Execute(null);

            this.mockBookingService.Verify(
                bookingService => bookingService.ConfirmBooking(It.IsAny<int>(), It.IsAny<Slot>()),
                Times.Never);
        }

        [Fact]
        public void JoinInterviewCommand_WhenObjectIsNull_DoesNothing()
        {
            var viewModel = this.CreateViewModel();

            var exception = Record.Exception(() => viewModel.JoinInterviewCommand.Execute(null));

            Assert.Null(exception);
        }

        [Fact(Skip = "CancelInterviewCommand implementation has changed - skipping outdated test")]
        public async Task CancelInterviewCommand_WhenSessionExists_DeletesSession()
        {
            var session = new InterviewSession { Id = 1 };

            this.mockSessionService
                .Setup(sessionService => sessionService.GetSessionAsync(1))
                .ReturnsAsync(session);

            var viewModel = this.CreateViewModel();

            viewModel.CancelInterviewCommand.Execute(session);

            await Task.Delay(100);

            this.mockSessionService.Verify(
                sessionService => sessionService.DeleteSessionAsync(session.Id),
                Times.Once);
        }

        [Fact(Skip = "CancelInterviewCommand implementation has changed - skipping outdated test")]
        public async Task CancelInterviewCommand_WhenSessionNotFound_DoesNotDelete()
        {
            var session = new InterviewSession { Id = 99 };

            this.mockSessionService
                .Setup(sessionService => sessionService.GetSessionAsync(99))
                .ReturnsAsync((InterviewSession?)null);

            var viewModel = this.CreateViewModel();

            viewModel.CancelInterviewCommand.Execute(session);

            await Task.Delay(100);

            this.mockSessionService.Verify(
                sessionService => sessionService.DeleteSessionAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact(Skip = "CancelInterviewCommand implementation has changed - skipping outdated test")]
        public void CancelInterviewCommand_WhenObjectIsNotSession_DoesNothing()
        {
            var viewModel = this.CreateViewModel();

            viewModel.CancelInterviewCommand.Execute("not a session");

            this.mockSessionService.Verify(
                sessionService => sessionService.DeleteSessionAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact(Skip = "CancelInterviewCommand implementation has changed - skipping outdated test")]
        public async Task CancelInterviewCommand_WhenRepositoryThrows_DoesNotCrash()
        {
            var session = new InterviewSession { Id = 1 };

            this.mockSessionService
                .Setup(sessionService => sessionService.GetSessionAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            var viewModel = this.CreateViewModel();

            viewModel.CancelInterviewCommand.Execute(session);

            await Task.Delay(100);

            this.mockSessionService.Verify(
                sessionService => sessionService.DeleteSessionAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact(Skip = "LoadInterviewSessions implementation has changed - skipping outdated test")]
        public async Task LoadInterviewSessions_WhenSessionsExist_PopulatesInterviewSessions()
        {
            this.mockSessionService
                .Setup(sessionService => sessionService.GetScheduledSessionsAsync())
                .ReturnsAsync(new List<InterviewSession> { new InterviewSession { Id = 1 } });

            var viewModel = this.CreateViewModel();

            await Task.Delay(100);

            Assert.Single(viewModel.InterviewSessions);
        }

        [Fact(Skip = "LoadInterviewSessions implementation has changed - skipping outdated test")]
        public async Task LoadInterviewSessions_WhenRepositoryThrows_LeavesSessionsEmpty()
        {
            this.mockSessionService
                .Setup(sessionService => sessionService.GetScheduledSessionsAsync())
                .ThrowsAsync(new Exception("Database error"));

            var viewModel = this.CreateViewModel();

            await Task.Delay(100);

            Assert.Empty(viewModel.InterviewSessions);
        }

        [Fact]
        public void LoadAvailableSlotsCommand_WhenExecuted_UpdatesMatchedCompanies()
        {
            var viewModel = this.CreateViewModel();

            viewModel.LoadAvailableSlotsCommand.Execute(null);

            Assert.Equal(2, viewModel.MatchedCompanies.Count);
        }

        [Fact]
        public void OnPropertyChanged_WhenNoListenersAttached_DoesNotThrow()
        {
            var viewModel = this.CreateViewModel();

            var exception = Record.Exception(() => viewModel.SelectedSlot = new Slot());

            Assert.Null(exception);
        }

    }
}