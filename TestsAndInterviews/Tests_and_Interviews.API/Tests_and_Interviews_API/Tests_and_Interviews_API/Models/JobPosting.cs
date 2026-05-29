namespace Tests_and_Interviews_API.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// JobPosting class represents a job posting created by a company, containing details about the job such as title, description, location, required skills, and other relevant information.
    /// It is associated with a Company entity and has a collection of JobSkill entities to represent the required skills for the job.
    /// The JobPosting class is mapped to the "jobs" table in the database, with properties corresponding to columns in the table.
    /// </summary>
    [Table("jobs")]
    public class JobPosting
    {
        /// <summary>
        /// Gets or sets the unique identifier for the job posting. This property is marked as the primary key and is mapped to the "job_id" column in the database.
        /// It is not auto-generated, meaning that it must be explicitly set when creating a new JobPosting instance.
        /// </summary>
        [Key]
        [Column("job_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the company associated with the job posting.
        /// This property is mapped to the "company_id" column in the database and represents a foreign key relationship to the Company entity.
        /// </summary>
        [Column("company_id")]
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the company associated with the job posting. This property represents a navigation property to the Company entity, allowing for access to the company's details and related information.
        /// </summary>
        public Company Company { get; set; } = null!;

        /// <summary>
        /// Gets or sets the photo associated with the job posting.
        /// This property is mapped to the "photo" column in the database and is of type nvarchar(max) to allow for storing a URL or base64-encoded string representing the photo.
        /// It can be used to display an image related to the job posting, such as a company logo or a relevant graphic.
        /// </summary>
        [Column("photo", TypeName = "nvarchar(max)")]
        public string? Photo { get; set; }

        /// <summary>
        /// Gets or sets the title of the job posting. This property is mapped to the "job_title" column in the database and has a maximum length of 255 characters.
        /// </summary>
        [Column("job_title", TypeName = "nvarchar(255)")]
        public string? JobTitle { get; set; }

        /// <summary>
        /// Gets or sets the industry field of the job posting. This property is mapped to the "industry_field" column in the database and has a maximum length of 255 characters.
        /// It represents a dropdown menu with options: IT, Business, Healthcare, Education, etc.
        /// </summary>
        [Column("industry_field", TypeName = "nvarchar(255)")]
        public string? IndustryField { get; set; }

        /// <summary>
        /// Gets or sets the job type of the job posting. This property is mapped to the "job_type" column in the database and has a maximum length of 255 characters.
        /// Type: dropdown menu with options - can be multiple choice (part-time, full-time, volunteer, internship, remote, hybrid, etc).
        /// </summary>
        [Column("job_type", TypeName = "nvarchar(255)")]
        public string? JobType { get; set; }

        /// <summary>
        /// Gets or sets the experience level of the job posting. This property is mapped to the "experience_level" column in the database and has a maximum length of 255 characters.
        /// It represents a dropdown menu with options: internship, entry level, mid-senior level, director, executive.
        /// </summary>
        [Column("experience_level", TypeName = "nvarchar(255)")]
        public string? ExperienceLevel { get; set; }

        /// <summary>
        /// Gets or sets the start date of the job posting. This property is mapped to the "start_date" column in the database and is of type date.
        /// </summary>
        [Column("start_date", TypeName = "date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the job posting. This property is mapped to the "end_date" column in the database and is of type date.
        /// </summary>
        [Column("end_date", TypeName = "date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the job description for the job posting.
        /// </summary>
        [Column("job_description", TypeName = "nvarchar(max)")]
        public string? JobDescription { get; set; }

        /// <summary>
        /// Gets or sets the job location for the job posting.
        /// </summary>
        [Column("job_location", TypeName = "nvarchar(255)")]
        public string? JobLocation { get; set; }

        /// <summary>
        /// Gets or sets the number of available positions for the job posting.
        /// </summary>
        [Column("available_positions")]
        public int AvailablePositions { get; set; }

        /// <summary>
        /// Gets or sets the time the jos posting was made. It is completed automatically with getTime().
        /// </summary>
        [Column("posted_at", TypeName = "datetime")]
        public DateTime? PostedAt { get; set; }

        /// <summary>
        /// Gets or sets the salary for the job posting. It must be a valid salary (positive).
        /// </summary>
        [Column("salary")]
        public int? Salary { get; set; }// <0

        /// <summary>
        /// Gets or sets the amount payed for the job posting. It is 0 by default.
        /// </summary>
        [Column("amount_payed")]
        public int? AmountPayed { get; set; }

        /// <summary>
        /// Gets or sets the deadline for the job posting.
        /// </summary>
        [Column("deadline", TypeName = "date")]
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Gets or sets the job skills fot the job posting.
        /// *Required skills: checkboxes with different skills options (Python, Java, C++, etc.) and a corresponding percentage representing the minimum required knowledge for the job;
        /// </summary>
        public System.Collections.Generic.ICollection<JobSkill> JobSkills { get; set; } = new System.Collections.Generic.List<JobSkill>();
    }
}
