using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class Question
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public TraitType Trait { get; set; }
    public int SortOrder { get; set; }
}
