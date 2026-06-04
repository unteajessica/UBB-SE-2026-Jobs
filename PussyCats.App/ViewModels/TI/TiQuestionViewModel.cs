using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;

namespace PussyCats.App.ViewModels.TI;

public enum TiQuestionType { SINGLE_CHOICE, MULTIPLE_CHOICE, TRUE_FALSE, TEXT, INTERVIEW }

public class TiQuestionViewModel : INotifyPropertyChanged
{
    private string textAnswer = string.Empty;
    private bool falseSelected;
    private bool trueSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int QuestionId { get; set; }
    public int DisplayNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public TiQuestionType Type { get; set; }
    public string TypeLabel => Type.ToString().Replace("_", " ");
    public ObservableCollection<TiOptionViewModel> Options { get; set; } = new();
    public Action? OnAnswerChanged { get; set; }

    public Visibility IsSingleChoice => Type == TiQuestionType.SINGLE_CHOICE ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsMultipleChoice => Type == TiQuestionType.MULTIPLE_CHOICE ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsTrueFalse => Type == TiQuestionType.TRUE_FALSE ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsText => Type == TiQuestionType.TEXT ? Visibility.Visible : Visibility.Collapsed;

    public string TrueFalseGroup => $"tf_{QuestionId}";

    public bool TrueSelected
    {
        get => trueSelected;
        set
        {
            trueSelected = value;
            if (value) falseSelected = false;
            Notify();
            Notify(nameof(FalseSelected));
            OnAnswerChanged?.Invoke();
        }
    }

    public bool FalseSelected
    {
        get => falseSelected;
        set
        {
            falseSelected = value;
            if (value) trueSelected = false;
            Notify();
            Notify(nameof(TrueSelected));
            OnAnswerChanged?.Invoke();
        }
    }

    public string TextAnswer
    {
        get => textAnswer;
        set
        {
            textAnswer = value;
            Notify();
            OnAnswerChanged?.Invoke();
        }
    }

    public string GetAnswerValue() => Type switch
    {
        TiQuestionType.SINGLE_CHOICE => Options.FirstOrDefault(o => o.IsSelected)?.Index.ToString() ?? string.Empty,
        TiQuestionType.MULTIPLE_CHOICE => "[" + string.Join(",", Options.Where(o => o.IsSelected).Select(o => o.Index)) + "]",
        TiQuestionType.TRUE_FALSE => TrueSelected ? "true" : FalseSelected ? "false" : string.Empty,
        TiQuestionType.TEXT => TextAnswer.Trim(),
        _ => string.Empty
    };

    public bool IsAnswered() => Type switch
    {
        TiQuestionType.SINGLE_CHOICE => Options.Any(o => o.IsSelected),
        TiQuestionType.MULTIPLE_CHOICE => Options.Any(o => o.IsSelected),
        TiQuestionType.TRUE_FALSE => TrueSelected || FalseSelected,
        TiQuestionType.TEXT => !string.IsNullOrWhiteSpace(TextAnswer),
        _ => false
    };

    private void Notify([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
