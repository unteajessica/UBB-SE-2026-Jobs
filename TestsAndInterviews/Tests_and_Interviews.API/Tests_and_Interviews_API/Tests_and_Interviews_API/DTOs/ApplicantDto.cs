namespace Tests_and_Interviews_API.Dtos
{
    using System;

    /// <summary>
    /// Represents an applicant's application to a job posting.
    /// </summary>
    public class ApplicantDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the applicant.
        /// </summary>
        public int ApplicantId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the job associated with the applicant's application.
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user who is the applicant.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the grade for the application test taken by the applicant.
        /// </summary>
        public decimal? AppTestGrade { get; set; }

        /// <summary>
        /// Gets or sets the grade for the applicant's CV.
        /// </summary>
        public decimal? CvGrade { get; set; }

        /// <summary>
        /// Gets or sets the grade for the company test taken by the applicant.
        /// </summary>
        public decimal? CompanyTestGrade { get; set; }

        /// <summary>
        /// Gets or sets the grade for the interview taken by the applicant.
        /// </summary>
        public decimal? InterviewGrade { get; set; }

        /// <summary>
        /// Gets or sets the overall status of the applicant's application.
        /// </summary>
        public string? ApplicationStatus { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the applicant applied for the job.
        /// </summary>
        public DateTime AppliedAt { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the company that recommended the applicant.
        /// </summary>
        public int? RecommendedFromCompanyId { get; set; }

        /// <summary>
        /// Gets or sets the URL of the applicant's CV file.
        /// </summary>
        public string? CvFileUrl { get; set; }
    }
}