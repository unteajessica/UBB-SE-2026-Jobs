using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.DTOs;

public class PersonalityTestAnswer
{
    public string QuestionText { get; set; } = string.Empty;

    public TraitType Trait { get; set; }

    public int SortOrder { get; set; }

    public int Answer { get; set; }
}
