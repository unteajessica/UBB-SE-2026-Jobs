namespace Tests_and_Interviews_API.Dtos
{
    /// <summary>
    /// Represents a skill that can be associated with job postings.
    /// </summary>
    public class SkillDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the skill.
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        public string SkillName { get; set; } = string.Empty;
    }
}