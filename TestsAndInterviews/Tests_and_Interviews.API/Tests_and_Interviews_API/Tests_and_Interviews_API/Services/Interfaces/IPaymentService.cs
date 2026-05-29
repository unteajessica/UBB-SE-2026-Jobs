namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Defines operations for managing job payments.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Updates the payment amount for the specified job.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="paymentAmount">The new payment amount to apply.</param>
        void UpdateJobPayment(int jobId, int paymentAmount);

        /// <summary>
        /// Retrieves all paid jobs matching the specified job type and experience level.
        /// </summary>
        /// <param name="jobType">The type of the job.</param>
        /// <param name="experienceLevel">The experience level required for the job.</param>
        /// <returns>A list of job payment information matching the specified criteria.</returns>
        List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel);

        /// <summary>
        /// Retrieves the email addresses of companies to notify about a new payment amount for the specified job.
        /// </summary>
        /// <param name="currentJobId">The unique identifier of the current job.</param>
        /// <param name="newPaymentAmount">The new payment amount to compare against.</param>
        /// <returns>A list of email addresses of companies to notify.</returns>
        List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount);

        /// <summary>
        /// Processes a payment for the specified job by updating the payment amount,
        /// fetching companies to notify, and sending notification emails.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="paymentAmount">The new payment amount to apply.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ProcessPaymentAsync(int jobId, int paymentAmount);
    }
}