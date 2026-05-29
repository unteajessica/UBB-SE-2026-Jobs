// <copyright file="NotificationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Services
{
    using System;
    using Microsoft.Toolkit.Uwp.Notifications;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// Provides methods to display Windows system notifications to the user.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IToastNotifier notifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="notifier">  An implementation of <see cref="IToastNotifier"/> used to display notifications.</param>
        public NotificationService(IToastNotifier notifier)
        {
            this.notifier = notifier;
        }

        /// <summary>
        /// Displays a formatted toast notification confirming an interview booking.
        /// </summary>
        /// <param name="companyName">The name of the company hosting the interview.</param>
        /// <param name="jobTitle">The specific position being interviewed for.</param>
        /// <param name="startTime">The date and start time of the interview.</param>
        /// <param name="endTime">The scheduled end time of the interview.</param>
        public void ShowBookingConfirmed(string companyName, string jobTitle, DateTime startTime, DateTime endTime)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText("Interview confirmed")
                    .AddText($"{companyName} - {jobTitle}")
                    .AddText($"{startTime:MMM dd yyyy h:mm tt} - {endTime:h:mm tt}")
                    .AddButton(new ToastButtonDismiss("Close"));
                this.notifier.Show(builder);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Notification failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays a generic toast notification with a custom title and message.
        /// </summary>
        /// <param name="title">The bold header text of the notification.</param>
        /// <param name="message">The body text providing additional details.</param>
        public void ShowSimpleNotification(string title, string message)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddButton(new ToastButtonDismiss("Close"));

                this.notifier.Show(builder);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Notification failed: {ex.Message}");
            }
        }
    }
}