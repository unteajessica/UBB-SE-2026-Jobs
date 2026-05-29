namespace Tests_and_Interviews_API.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Applicant class represents a user's application to a job posting, including their grades for various evaluation criteria and the status of their application.
    /// It contains properties for the applicant's ID, associated job and user, grades for application test, CV, company test, and interview, as well as the overall application status and the date of application.
    /// The Applicant class is mapped to the "applicants" table in the database, with a primary key of ApplicantId that is not auto-generated. It also includes navigation properties for related entities such as JobPosting,
    /// User, and Company (for recommendations).
    /// </summary>
    [Table("applicants")]
    public class Applicant
    {
        /// <summary>
        /// Gets or sets the unique identifier for the applicant. This property is marked as the primary key and is mapped to the "applicant_id" column in the database.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("applicant_id")]
        public int ApplicantId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the job associated with the applicant's application. This property is mapped to the "job_id" column in the database and represents a foreign key relationship to the JobPosting entity.
        /// </summary>
        [Column("job_id")]
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets the job associated with the applicant's application. This property represents a navigation property to the JobPosting entity, allowing for access to the job's details and related information.
        /// </summary>
        public JobPosting Job { get; set; } = null!;

        /// <summary>
        /// Gets or sets the unique identifier for the user who is the applicant. This property is mapped to the "user_id" column in the database and represents a foreign key relationship to the User entity.
        /// </summary>
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who is the applicant. This property represents a navigation property to the User entity, allowing for access to the user's details and related information.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the grade for the application test taken by the applicant. This property is mapped to the "app_test_grade" column in the database and is of type decimal with a precision of 5 and a scale of 2, allowing for values such as 85.50.
        /// </summary>
        [Column("app_test_grade", TypeName = "decimal(5,2)")]
        public decimal? AppTestGrade { get; set; }

        /// <summary>
        /// Gets or sets the grade for the applicant's CV. This property is mapped to the "cv_grade" column in the database and is of type decimal with a precision of 5 and a scale of 2, allowing for values such as 90.00.
        /// </summary>
        [Column("cv_grade", TypeName = "decimal(5,2)")]
        public decimal? CvGrade { get; set; }

        /// <summary>
        /// Gets or sets the grade for the company test taken by the applicant. This property is mapped to the "company_test_grade" column in the database and is of type decimal with a precision of 5 and a scale of 2, allowing for values such as 85.50.
        /// </summary>
        [Column("company_test_grade", TypeName = "decimal(5,2)")]
        public decimal? CompanyTestGrade { get; set; }

        /// <summary>
        /// Gets or sets the grade for the interview taken by the applicant. This property is mapped to the "interview_grade" column in the database and is of type decimal with a precision of 5 and a scale of 2, allowing for values such as 88.75.
        /// </summary>
        [Column("interview_grade", TypeName = "decimal(5,2)")]
        public decimal? InterviewGrade { get; set; }

        /// <summary>
        /// Gets or sets the overall status of the applicant's application. This property is mapped to the "application_status" column in the database and has a maximum length of 50 characters.
        /// status: dropdown menu with options - can be multiple choice ("Failed", "On Hold", "Accepted", "Recommended").
        /// </summary>
        [Column("application_status", TypeName = "nvarchar(50)")]
        public string? ApplicationStatus { get; set; } = null;

        /// <summary>
        /// Gets or sets the date and time when the applicant applied for the job. This property is mapped to the "applied_at" column in the database and is of type datetime, representing the timestamp of when the application was submitted.
        /// </summary>
        [Column("applied_at", TypeName = "datetime")]
        public DateTime AppliedAt { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the company that recommended the applicant. This property is mapped to the "recommended_from_company_id" column in
        /// </summary>
        [Column("recommended_from_company_id")]
        public int? RecommendedFromCompanyId { get; set; }

        /// <summary>
        /// Gets or sets the company that recommended the applicant. This property represents a navigation property to the Company entity, allowing for access to the company's details and related information.
        /// </summary>
        public Company? RecommendedFromCompany { get; set; }


        /// <summary>
        /// Gets or sets the URL of the applicant's CV file. This property is mapped to the "cv_file_url" column in the database and has a maximum length of 500 characters. It can be used to store a link to the applicant's CV file, allowing recruiters to access and review it as part of the application process.
        /// </summary>
        [Column("cv_file_url", TypeName = "nvarchar(500)")]
        public string? CvFileUrl { get; set; }
    }
}
