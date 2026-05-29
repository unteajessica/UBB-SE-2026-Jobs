namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Toolkit.Uwp.Notifications;

    /// <summary>
    /// Defines a contract for sending system-level notifications to the user.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Triggers a high-priority notification to confirm a scheduled interview.
        /// </summary>
        /// <param name="companyName">The name of the organization conducting the interview.</param>
        /// <param name="jobTitle">The specific role or position title.</param>
        /// <param name="startTime">The starting date and time of the appointment.</param>
        /// <param name="endTime">The expected conclusion time of the appointment.</param>
        public void ShowBookingConfirmed(string companyName, string jobTitle, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Triggers a standard notification with a custom title and body text.
        /// </summary>
        /// <param name="title">The primary heading for the notification toast.</param>
        /// <param name="message">The descriptive content or body text of the notification.</param>
        public void ShowSimpleNotification(string title, string message);
    }
}
