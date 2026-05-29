namespace Tests_and_Interviews_API.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("JobSkills")]
    public class JobSkill
    {
        [Column("SkillId")]
        public int SkillId { get; set; }

        public Skill Skill { get; set; } = null!;

        [Column("JobId")]
        public int JobId { get; set; }

        public JobPosting Job { get; set; } = null!;

        [Column("RequiredLevel")]
        public int RequiredPercentage { get; set; }
    }
}
