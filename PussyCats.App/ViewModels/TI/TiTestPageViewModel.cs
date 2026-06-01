using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.UI.Xaml;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public class TiTestPageViewModel : INotifyPropertyChanged
{
    private readonly ITiTestService testService;
    private string testTitle = string.Empty;
    private TimeSpan timeLeft = TimeSpan.FromMinutes(30);
    private DispatcherTimer? timer;
    private int answeredCount;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TiTestPageViewModel(ITiTestService testService)
    {
        this.testService = testService;
    }

    public ObservableCollection<TiQuestionViewModel> Questions { get; } = new();

    public string TestTitle
    {
        get => testTitle;
        set { testTitle = value; Notify(); }
    }

    public string TimerDisplay => timeLeft.ToString(@"mm\:ss");
    public Action? OnTimerExpired { get; set; }

    public int AnsweredCount
    {
        get => answeredCount;
        set { answeredCount = value; Notify(); }
    }

    public int TotalCount => Questions.Count;
    public bool AlreadyAttempted { get; private set; }
    public int UserId { get; set; }
    public int TestId { get; set; }

    public async Task LoadAsync(int testId, int userId)
    {
        TestId = testId;
        UserId = userId;

        var test = await testService.GetByIdAsync(testId);
        if (test == null) return;

        TestTitle = test.Title;

        try
        {
            await testService.StartAttemptAsync(userId, testId);
        }
        catch (InvalidOperationException)
        {
            AlreadyAttempted = true;
            return;
        }
        catch { }

        var questions = await testService.GetQuestionsByTestIdAsync(testId);
        int index = 1;
        foreach (var q in questions)
        {
            var type = q.QuestionType switch
            {
                "SINGLE_CHOICE" => TiQuestionType.SINGLE_CHOICE,
                "MULTIPLE_CHOICE" => TiQuestionType.MULTIPLE_CHOICE,
                "TRUE_FALSE" => TiQuestionType.TRUE_FALSE,
                "INTERVIEW" => TiQuestionType.INTERVIEW,
                _ => TiQuestionType.TEXT
            };

            if (type == TiQuestionType.INTERVIEW) continue;

            var qvm = new TiQuestionViewModel
            {
                QuestionId = q.Id,
                DisplayNumber = index++,
                QuestionText = q.QuestionText,
                Type = type,
            };

            if (type == TiQuestionType.SINGLE_CHOICE || type == TiQuestionType.MULTIPLE_CHOICE)
            {
                var options = new List<string>();
                if (!string.IsNullOrEmpty(q.OptionsJson))
                {
                    try { options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new(); }
                    catch { }
                }

                for (int i = 0; i < options.Count; i++)
                {
                    qvm.Options.Add(new TiOptionViewModel
                    {
                        Text = options[i],
                        Index = i,
                        GroupName = $"q_{q.Id}",
                        OnSelectionChanged = UpdateAnsweredCount,
                    });
                }
            }

            qvm.OnAnswerChanged = UpdateAnsweredCount;
            Questions.Add(qvm);
        }

        Notify(nameof(TotalCount));
        StartTimer();
    }

    public void StopTimer() => timer?.Stop();

    public async Task<float> SubmitAsync()
    {
        StopTimer();
        var answers = Questions
            .Select(q => new TiAnswerDto { QuestionId = q.QuestionId, Value = q.GetAnswerValue() })
            .Where(a => !string.IsNullOrEmpty(a.Value))
            .ToList();

        return await testService.SubmitAttemptAsync(UserId, TestId, answers);
    }

    private void StartTimer()
    {
        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) =>
        {
            timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
            Notify(nameof(TimerDisplay));
            if (timeLeft <= TimeSpan.Zero)
            {
                timer.Stop();
                OnTimerExpired?.Invoke();
            }
        };
        timer.Start();
    }

    private void UpdateAnsweredCount() =>
        AnsweredCount = Questions.Count(q => q.IsAnswered());

    private void Notify([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
