using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels;

public class QuestionViewModel : ObservableObject
{
    private int? selectedAnswer;

    public QuestionViewModel(Question question)
    {
        Question = question;
    }

    public Question Question { get; }

    public int? SelectedAnswer
    {
        get => selectedAnswer;
        set
        {
            if (SetProperty(ref selectedAnswer, value))
            {
                OnPropertyChanged(nameof(IsAnswered));
            }
        }
    }

    public bool IsAnswered => SelectedAnswer is not null;
}
