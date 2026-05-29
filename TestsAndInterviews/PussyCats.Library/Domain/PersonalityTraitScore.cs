using PussyCats.Library.Domain.Enums;
using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class PersonalityTraitScore
{
    public int PersonalityTraitScoreId { get; set; }

    public PersonalityTestResult PersonalityTestResult { get; set; } = null!;

    public TraitType Trait { get; set; }
    public int Score { get; set; }
}
