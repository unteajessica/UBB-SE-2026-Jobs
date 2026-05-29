namespace Tests_and_Interviews.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// JobPaymentInfo class represents the payment information related to a specific job, including the company name, job title, and the amount paid for that job.
    /// This class is used to encapsulate the details of a job payment, allowing for easy access and management of payment information associated with different job postings.
    /// </summary>
    public class JobPaymentInfo
    {
        /// <summary>
        /// Gets or sets the name of the company associated with the job.
        /// This property represents the name of the company that is offering the job, allowing for identification of the employer and providing context for the payment information related to that job.
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the title of the job associated with the payment information.
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Gets or sets the amount paid for the job. This property represents the payment amount associated with the job, allowing for tracking and management of payments related to different job postings.
        /// </summary>
        public int AmountPayed { get; set; }
    }
}
