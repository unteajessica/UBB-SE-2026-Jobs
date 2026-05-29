namespace Tests_and_Interviews.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Skill entity representing a specific skill that can be associated with job postings. Contains properties for the skill's unique identifier and name, as well as a collection of related JobSkill entities to establish a many-to-many relationship with job postings.
    /// </summary>
    [Table("skills")]
    public class Skill
    {
        /// <summary>
        /// Gets or sets the unique identifier for the skill. This property is marked as the primary key and is mapped to the "skill_id" column in the database.
        /// </summary>
        [Key]
        [Column("skill_id")]
        public int SkillId { get; set; }

        /// <summary>
        /// Gets or sets the name of the skill. This property is mapped to the "skill_name" column in the database and has a maximum length of 255 characters. It is initialized to an empty string to ensure it is never null.
        /// </summary>

        [Column("skill_name", TypeName = "nvarchar(255)")]
        public string SkillName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of JobSkill entities that represent the many-to-many relationship between skills and job postings. This collection allows for navigation from a skill to the job postings that require it. It is initialized to an empty list to ensure it is never null.
        /// </summary>
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    }
}
