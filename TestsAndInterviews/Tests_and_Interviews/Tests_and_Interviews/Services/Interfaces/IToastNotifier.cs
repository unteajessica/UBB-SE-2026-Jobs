namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Toolkit.Uwp.Notifications;

    /// <summary>
    /// Defines a wrapper for the Windows Toast notification system.
    /// This interface allows the notification display logic to be mocked in unit tests.
    /// </summary>
    public interface IToastNotifier
    {
        void Show(ToastContentBuilder builder);
    }

    /// <summary>
    /// Displays the toast notification defined by the provided <see cref="ToastContentBuilder"/>.
    /// </summary>
    /// <param name="builder">The builder containing the visual and functional configuration of the toast.</param>
    public class WindowsToastNotifier : IToastNotifier
    {
        /// <summary>
        /// Triggers the system to display the notification to the user.
        /// </summary>
        /// <param name="builder">The configured notification content.</param>
        public void Show(ToastContentBuilder builder) => builder.Show();
    }
}
