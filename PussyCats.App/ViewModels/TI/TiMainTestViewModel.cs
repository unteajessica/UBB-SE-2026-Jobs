using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public class TiMainTestViewModel : INotifyPropertyChanged
{
    private readonly ITiTestService testService;
    private bool isLoading;
    private TiTestCardViewModel? selectedTest;

    public event PropertyChangedEventHandler? PropertyChanged;

    public TiMainTestViewModel(ITiTestService testService)
    {
        this.testService = testService;
    }

    public ObservableCollection<TiTestCardViewModel> Tests { get; } = new();

    public bool IsLoading
    {
        get => isLoading;
        set { isLoading = value; Notify(); Notify(nameof(NoTestsVisible)); }
    }

    public TiTestCardViewModel? SelectedTest
    {
        get => selectedTest;
        set
        {
            if (selectedTest != null) selectedTest.IsSelected = false;
            selectedTest = value;
            if (selectedTest != null) selectedTest.IsSelected = true;
            Notify();
        }
    }

    public Visibility NoTestsVisible =>
        !IsLoading && Tests.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public async Task LoadTestsAsync()
    {
        IsLoading = true;
        Tests.Clear();

        var categories = new[] { "Programming", "Database", "Computer Science" };
        foreach (var category in categories)
        {
            var tests = await testService.GetByCategoryAsync(category);
            foreach (var test in tests)
            {
                Tests.Add(new TiTestCardViewModel
                {
                    TestId = test.Id,
                    Title = test.Title,
                    Category = test.Category,
                    QuestionTypeLabel = test.QuestionTypeLabel,
                });
            }
        }

        IsLoading = false;
        Notify(nameof(NoTestsVisible));
    }

    private void Notify([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
