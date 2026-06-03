using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiTestPage : Page
{
    public TiTestPageViewModel ViewModel { get; }

    public TiTestPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiTestPageViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        int testId = e.Parameter is int id ? id : 0;
        var session = App.Services.GetRequiredService<SessionContext>();
        int userId = session.UserId;

        ViewModel.OnTimerExpired = async () =>
        {
            await ViewModel.SubmitAsync();
            ShowResultDialog("Time's up!", "Your test has been auto-submitted.");
            Frame.Navigate(typeof(TiMainTestPage));
        };

        await ViewModel.LoadAsync(testId, userId);

        if (ViewModel.AlreadyAttempted)
        {
            ShowResultDialog("Already Taken", "You have already completed this test.");
            Frame.GoBack();
        }
    }

    private void BackToTests_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.StopTimer();
        Frame.Navigate(typeof(TiMainTestPage));
    }

    private async void SubmitTest_Click(object sender, RoutedEventArgs e)
    {
        float score = await ViewModel.SubmitAsync();
        ShowResultDialog("Test Submitted!", $"Your score: {score:0.##}");
        Frame.Navigate(typeof(TiMainTestPage));
    }

    private async void ShowResultDialog(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        await dialog.ShowAsync();
    }
}
