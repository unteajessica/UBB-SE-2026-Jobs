namespace Tests_and_Interviews.Models.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The Test class represents a test or quiz that candidates can take. 
    /// It contains properties for the test's title, category, creation date, and related questions and attempts.
    /// </summary>
    [Table("Tests")]
    public class Test
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title associated with this entity.
        /// </summary>
        [Column("title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category associated with the entity.
        /// </summary>
        [Column("category")]
        [MaxLength(200)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the collection of questions associated with this instance.
        /// </summary>
        public List<Question> Questions { get; set; } = [];
        /// <summary>
        /// Gets or sets the collection of test attempts associated with this instance.
        /// </summary>
        public List<TestAttempt> Attempts { get; set; } = [];
    }
}