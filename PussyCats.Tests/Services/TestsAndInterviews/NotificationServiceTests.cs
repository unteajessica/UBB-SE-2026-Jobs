// <copyright file="NotificationServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Toolkit.Uwp.Notifications;
    using Moq;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="NotificationService"/> class.
    /// These tests are designed as unit tests by mocking the IToastNotifier to avoid dependencies on the OS notification system.
    /// </summary>
    public class NotificationServiceTests
    {
        private readonly Mock<IToastNotifier> mockNotifier;
        private readonly NotificationService svc;

        public NotificationServiceTests()
        {
            this.mockNotifier = new Mock<IToastNotifier>();
            this.svc = new NotificationService(this.mockNotifier.Object);
        }

        [Fact]
        public void ShowBookingConfirmed_WithValidInputs_DoesNotThrow()
        {
            var company = "Bosch";
            var title = "Software Engineer Intern";
            var start = new DateTime(2026, 5, 1, 14, 0, 0);
            var end = new DateTime(2026, 5, 1, 15, 0, 0);

            var ex = Record.Exception(() => this.svc.ShowBookingConfirmed(company, title, start, end));

            Assert.Null(ex);
            this.mockNotifier.Verify(n => n.Show(It.IsAny<ToastContentBuilder>()), Times.Once);
        }

        [Fact]
        public void ShowSimpleNotification_WithValidInputs_DoesNotThrow()
        {
            var title = "Reminder";
            var message = "This is a test notification.";

            var ex = Record.Exception(() => this.svc.ShowSimpleNotification(title, message));

            Assert.Null(ex);
            this.mockNotifier.Verify(n => n.Show(It.IsAny<ToastContentBuilder>()), Times.Once);
        }

        [Fact]
        public void ShowBookingConfirmed_WithNullOrEmptyInputs_DoesNotThrow()
        {
            string company = null;
            string title = string.Empty;
            var start = DateTime.Now;
            var end = DateTime.Now.AddMinutes(30);

            var ex = Record.Exception(() => this.svc.ShowBookingConfirmed(company, title, start, end));

            Assert.Null(ex);
        }

        [Fact]
        public void ShowSimpleNotification_WithNullValues_DoesNotThrow()
        {
            string title = null;
            string message = null;

            var ex = Record.Exception(() => this.svc.ShowSimpleNotification(title, message));

            Assert.Null(ex);
        }

        [Fact]
        public void ShowBookingConfirmed_WhenEndTimeIsBeforeStartTime_DoesNotThrow()
        {
            var start = new DateTime(2026, 12, 31, 23, 0, 0);
            var end = new DateTime(2026, 1, 1, 1, 0, 0);

            var ex = Record.Exception(() => this.svc.ShowBookingConfirmed("Test", "Test", start, end));

            Assert.Null(ex);
        }

        [Fact]
        public async Task ShowSimpleNotification_CalledFromDifferentThread_DoesNotThrow()
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                await Task.Run(() => this.svc.ShowSimpleNotification("Thread Test", "From Background"));
            });

            Assert.Null(ex);
            this.mockNotifier.Verify(n => n.Show(It.IsAny<ToastContentBuilder>()), Times.Once);
        }

        [Fact]
        public void ShowSimpleNotification_WhenToastFails_ExecutesCatchBlock()
        {
            this.mockNotifier.Setup(n => n.Show(It.IsAny<ToastContentBuilder>()))
                .Throws(new Exception("Windows Notification Service Unavailable"));

            var ex = Record.Exception(() => this.svc.ShowSimpleNotification("Title", "Message"));

            Assert.Null(ex);
            this.mockNotifier.Verify(n => n.Show(It.IsAny<ToastContentBuilder>()), Times.Once);
        }

        [Fact]
        public void ShowBookingConfirmed_WhenToastFails_ExecutesCatchBlock()
        {
            this.mockNotifier.Setup(n => n.Show(It.IsAny<ToastContentBuilder>()))
                .Throws(new Exception("OS Error"));

            var ex = Record.Exception(() =>
                this.svc.ShowBookingConfirmed("Company", "Job", DateTime.Now, DateTime.Now.AddHours(1)));

            Assert.Null(ex);
            this.mockNotifier.Verify(n => n.Show(It.IsAny<ToastContentBuilder>()), Times.Once);
        }
    }
}