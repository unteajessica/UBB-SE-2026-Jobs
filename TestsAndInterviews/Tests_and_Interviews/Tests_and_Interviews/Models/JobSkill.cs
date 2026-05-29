namespace Tests_and_Interviews.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// JobSkill class represents the association between a job posting and a required skill, including the percentage of proficiency required for that skill in the context of the job.
    /// It contains properties for the skill ID, job ID, and required percentage, as well as navigation properties to the related Skill and JobPosting entities.
    /// This class is mapped to the "job_skills" table in the database, with columns for skill_id, job_id, and required_percentage.
    /// </summary>
    [Table("job_skills")]
    public class JobSkill
    {
        /// <summary>
        /// Gets or sets the unique identifier for the skill associated with the job posting.
        /// This property is mapped to the "skill_id" column in the database and represents a foreign key relationship to the Skill entity.
        /// It allows for navigation from a JobSkill instance to the related Skill entity, providing access to the skill's details such as its name and associated job postings.
        /// </summary>
        [Column("skill_id")]
        public int SkillId { get; set; }

        /// <summary>
        /// Gets or sets the skill associated with the job posting. This property represents a navigation property to the Skill entity, allowing for access to the skill's details and related information.
        /// </summary>
        public Skill Skill { get; set; } = null!;


        /// <summary>
        /// Gets or sets the unique identifier for the job posting associated with the skill requirement.
        /// This property is mapped to the "job_id" column in the database and represents a foreign key relationship to the JobPosting entity.
        /// </summary>
        [Column("job_id")]
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets the job posting associated with the skill requirement. This property represents a navigation property to the JobPosting entity, allowing for access to the job's details and related information.
        /// </summary>
        public JobPosting Job { get; set; } = null!;

        /// <summary>
        /// Gets or sets the percentage of proficiency required for the skill in the context of the job posting.
        /// This property is mapped to the "required_percentage" column in the database and represents how much of the skill is required for the job, with a value typically ranging from 0 to 100.
        /// </summary>
        [Column("required_percentage")]
        public int RequiredPercentage { get; set; }
    }
}
