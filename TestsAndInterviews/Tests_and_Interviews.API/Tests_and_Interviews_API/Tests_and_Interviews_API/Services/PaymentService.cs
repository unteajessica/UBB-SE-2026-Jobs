namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing job payments.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private const int EmptyCollectionCount = 0;
        private const string AdminEmailAddress = "carla.draghiciu@cnglsibiu.ro";
        private const string AdminEmailDisplayName = "Job Portal Admin";
        private const string AdminEmailPassword = "[REDACTED_PASSWORD]";
        private const string SmtpHostAddress = "smtp.gmail.com";
        private const int SmtpHostPort = 587;
        private const int SmtpTimeoutMilliseconds = 60000;
        private const string NotificationEmailSubject = "Job Promotion Alert!";
        private const string EmailSentDebugMessagePrefix = "Email sent to ";
        private const string EmailFailedDebugMessagePrefix = "Failed to send email: ";
        private readonly IPaymentRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access payment data. Cannot be null.</param>
        public PaymentService(IPaymentRepository repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Processes a payment for the specified job by updating the payment amount,
        /// fetching companies to notify, and sending notification emails.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="paymentAmount">The new payment amount to apply.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ProcessPaymentAsync(int jobId, int paymentAmount)
        {
            this._repository.UpdateJobPayment(jobId, paymentAmount);

            List<string> emailsToNotify = this._repository.GetCompaniesToNotify(jobId, paymentAmount);

            if (emailsToNotify != null && emailsToNotify.Count > EmptyCollectionCount)
            {
                await this.SendNotificationEmailsAsync(emailsToNotify, paymentAmount);
            }
        }

        /// <summary>
        /// Updates the payment amount for the specified job.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="paymentAmount">The new payment amount to apply.</param>
        public void UpdateJobPayment(int jobId, int paymentAmount)
        {
            this._repository.UpdateJobPayment(jobId, paymentAmount);
        }

        /// <summary>
        /// Retrieves all paid jobs matching the specified job type and experience level.
        /// </summary>
        /// <param name="jobType">The type of the job.</param>
        /// <param name="experienceLevel">The experience level required for the job.</param>
        /// <returns>A list of job payment information matching the specified criteria.</returns>
        public List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel)
        {
            return this._repository.GetPaidJobs(jobType, experienceLevel);
        }

        /// <summary>
        /// Retrieves the email addresses of companies to notify about a new payment amount for the specified job.
        /// </summary>
        /// <param name="currentJobId">The unique identifier of the current job.</param>
        /// <param name="newPaymentAmount">The new payment amount to compare against.</param>
        /// <returns>A list of email addresses of companies to notify.</returns>
        public List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount)
        {
            return this._repository.GetCompaniesToNotify(currentJobId, newPaymentAmount);
        }

        /// <summary>
        /// Sends notification emails to the specified addresses informing them of a new competing payment amount.
        /// </summary>
        /// <param name="emails">The list of email addresses to notify.</param>
        /// <param name="newAmount">The new payment amount to include in the notification.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SendNotificationEmailsAsync(List<string> emails, int newAmount)
        {
            try
            {
                var fromAddress = new MailAddress(AdminEmailAddress, AdminEmailDisplayName);
                using (var smtpClient = new SmtpClient
                {
                    Host = SmtpHostAddress,
                    Port = SmtpHostPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, AdminEmailPassword),
                    Timeout = SmtpTimeoutMilliseconds,
                })
                {
                    foreach (string email in emails)
                    {
                        var toAddress = new MailAddress(email);
                        string notificationBody = $"Hello, \n\nJust letting you know that a competitor has placed a bid of ${newAmount} on a job that shares the same Type and Experience Level as yours. Consider increasing your budget to stay competitive!";
                        using (var mailMessage = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = NotificationEmailSubject,
                            Body = notificationBody,
                        })
                        {
                            await smtpClient.SendMailAsync(mailMessage);
                            System.Diagnostics.Debug.WriteLine($"{EmailSentDebugMessagePrefix}{email}!");
                        }
                    }
                }
            }
            catch (System.Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"{EmailFailedDebugMessagePrefix}{exception.Message}");
            }
        }
    }
}