namespace Tests_and_Interviews.Dtos
{
    /// <summary>
    /// Represents the payment information related to a specific job.
    /// </summary>
    public class JobPaymentInfoDto
    {
        /// <summary>
        /// Gets or sets the name of the company associated with the job.
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the title of the job associated with the payment information.
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Gets or sets the amount paid for the job.
        /// </summary>
        public int AmountPayed { get; set; }
    }
}