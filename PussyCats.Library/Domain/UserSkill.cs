using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class UserSkill
{
    //public int UserId { get; set; }
    [JsonIgnore] public User User { get; set; } = null!;

    //public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public int Score { get; set; }
    public bool IsVerified { get; set; }

    /// <summary>
    /// Set when the user verifies the skill via a passing skill test (i.e. when
    /// <see cref="IsVerified"/> transitions to true). Null on initial self-claim.
    /// Re-taking a test updates this to the latest verification date.
    /// </summary>
    public DateOnly? AchievedDate { get; set; }
}
