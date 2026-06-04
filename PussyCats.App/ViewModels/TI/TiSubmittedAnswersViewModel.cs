using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiSubmittedAnswersViewModel : DispatchableObservableObject
{
    private readonly ITiTestService testService;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string testTitle = string.Empty;

    public ObservableCollection<TiAnswerReviewItem> ReviewItems { get; } = new();

    public TiSubmittedAnswersViewModel(ITiTestService testService)
    {
        this.testService = testService;
    }

    public async Task LoadAsync(int testId, int attemptId)
    {
        IsLoading = true;
        ReviewItems.Clear();

        var test = await testService.GetByIdAsync(testId);
        TestTitle = test?.Title ?? string.Empty;

        var answers = await testService.GetAnswersByAttemptAsync(attemptId);
        var questions = await testService.GetQuestionsByTestIdAsync(testId);
        var questionMap = questions.ToDictionary(q => q.Id);

        foreach (var answer in answers)
        {
            if (!questionMap.TryGetValue(answer.QuestionId, out var question)) continue;

            var (status, earnedScore) = ParseStoredValue(answer.Value);
            string userAnswerDisplay = status == "Correct"
                ? "—"
                : FormatAnswerValue(answer.Value, question);

            ReviewItems.Add(new TiAnswerReviewItem
            {
                QuestionText = question.QuestionText,
                UserAnswerDisplay = userAnswerDisplay,
                CorrectAnswerDisplay = FormatCorrectAnswer(question),
                EarnedScoreDisplay = earnedScore.ToString("0.##", CultureInfo.InvariantCulture),
                MaxScoreDisplay = question.QuestionScore.ToString("0.##", CultureInfo.InvariantCulture),
                Status = status,
            });
        }

        IsLoading = false;
    }

    private static (string Status, float Score) ParseStoredValue(string value)
    {
        if (value.StartsWith("CORRECT:", StringComparison.OrdinalIgnoreCase))
        {
            float.TryParse(value["CORRECT:".Length..], NumberStyles.Float, CultureInfo.InvariantCulture, out float s);
            return ("Correct", s);
        }
        if (value.StartsWith("PARTIAL:", StringComparison.OrdinalIgnoreCase))
        {
            float.TryParse(value["PARTIAL:".Length..], NumberStyles.Float, CultureInfo.InvariantCulture, out float s);
            return ("Partial", s);
        }
        return ("Incorrect", 0f);
    }

    private static string FormatAnswerValue(string rawValue, TiQuestionDto question)
    {
        return question.QuestionType switch
        {
            "SINGLE_CHOICE" => ResolveOptionText(rawValue.Trim(), question.OptionsJson) ?? rawValue,
            "MULTIPLE_CHOICE" => ResolveMultipleOptionText(rawValue, question.OptionsJson),
            _ => rawValue
        };
    }

    private static string FormatCorrectAnswer(TiQuestionDto question)
    {
        if (string.IsNullOrEmpty(question.QuestionAnswer)) return "—";
        return question.QuestionType switch
        {
            "SINGLE_CHOICE" => ResolveOptionText(question.QuestionAnswer.Trim(), question.OptionsJson) ?? question.QuestionAnswer,
            "MULTIPLE_CHOICE" => ResolveMultipleOptionText(question.QuestionAnswer, question.OptionsJson),
            _ => question.QuestionAnswer
        };
    }

    private static string? ResolveOptionText(string indexStr, string? optionsJson)
    {
        if (string.IsNullOrEmpty(optionsJson)) return null;
        try
        {
            var opts = JsonSerializer.Deserialize<List<string>>(optionsJson);
            if (opts != null && int.TryParse(indexStr, out int idx) && idx >= 0 && idx < opts.Count)
                return opts[idx];
        }
        catch { }
        return null;
    }

    private static string ResolveMultipleOptionText(string indicesStr, string? optionsJson)
    {
        if (string.IsNullOrEmpty(optionsJson)) return indicesStr;
        try
        {
            var opts = JsonSerializer.Deserialize<List<string>>(optionsJson) ?? [];
            var labels = indicesStr.Trim().TrimStart('[').TrimEnd(']')
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => int.TryParse(p.Trim(), out int i) && i >= 0 && i < opts.Count ? opts[i] : p.Trim())
                .Where(s => !string.IsNullOrEmpty(s));
            return string.Join(", ", labels);
        }
        catch { }
        return indicesStr;
    }
}
