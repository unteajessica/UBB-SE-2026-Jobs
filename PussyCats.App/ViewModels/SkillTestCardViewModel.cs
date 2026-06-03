using System;
using PussyCats.App.Dtos.TI;
using PussyCats.Library.Domain;
using PussyCats.Library.Services;

namespace PussyCats.App.ViewModels;

/// <summary>
/// Display model for one TI skill test on the Skill Tests dashboard. Built from a TI test
/// plus the current user's attempt (if any). Replaces the former PussyCats SkillTest badge
/// card whose "retake" assigned a random score; results now come from the real TI engine.
/// </summary>
public class SkillTestCardViewModel
{
    public SkillTestCardViewModel(TiTestDto test, TiTestAttemptDto? attempt, float maxPossibleScore)
    {
        TestId = test.Id;
        Title = test.Title;
        Category = test.Category;

        bool isCompleted = ViewModelSupport.IsTiAttemptCompleted(attempt);
        bool isInProgress = !isCompleted && attempt is not null &&
            (Mentions(attempt.Status, "progress") || attempt.StartedAt is not null);

        if (isCompleted)
        {
            int percentage = ViewModelSupport.TiPercentage(attempt!.Score, maxPossibleScore);
            Status = "Completed";
            ScoreText = $"SCORE: {percentage}%";
            DateText = attempt.CompletedAt is { } completed ? completed.ToString("dd.MM.yyyy") : string.Empty;
            Badge = SimpleModelOperations.AssignTier(percentage);
            CanTakeTest = false; // TI tests are once-only
            ActionLabel = "COMPLETED";
        }
        else if (isInProgress)
        {
            Status = "In progress";
            ScoreText = "Not scored yet";
            DateText = attempt!.StartedAt is { } started ? $"Started {started:dd.MM.yyyy}" : string.Empty;
            CanTakeTest = true;
            ActionLabel = "CONTINUE";
        }
        else
        {
            Status = "Available";
            ScoreText = "Not taken yet";
            DateText = string.Empty;
            CanTakeTest = true;
            ActionLabel = "TAKE TEST";
        }
    }

    public int TestId { get; }
    public string Title { get; }
    public string Category { get; }
    public string Status { get; }
    public string ScoreText { get; }
    public string DateText { get; }
    public Badge? Badge { get; }
    public bool CanTakeTest { get; }
    public string ActionLabel { get; }

    private static bool Mentions(string? status, string token) =>
        status is not null && status.Contains(token, StringComparison.OrdinalIgnoreCase);
}
