using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.DTOs;
using Windows.UI;

namespace PussyCats_App.Views.Candidate;

public sealed partial class UserStatusPage : Page
{
    private static readonly SolidColorBrush ActiveBg   = new(Color.FromArgb(255, 30, 30, 30));
    private static readonly SolidColorBrush ActiveFg   = new(Colors.White);
    private static readonly SolidColorBrush ActiveBdr  = new(Color.FromArgb(255, 30, 30, 30));
    private static readonly SolidColorBrush InactiveBg  = new(Colors.White);
    private static readonly SolidColorBrush InactiveFg  = new(Colors.Black);
    private static readonly SolidColorBrush InactiveBdr = new(Colors.Black);

    private readonly UserStatusViewModel viewModel;

    public UserStatusPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<UserStatusViewModel>();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void OnLoaded(object sender, RoutedEventArgs eventArguments)
    {
        SetActiveFilter(FilterAll);
        await viewModel.LoadMatchesAsync();
    }

    private void Filter_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is not Button btn) return;
        SetActiveFilter(btn);
        viewModel.ApplyFilter(btn.Tag?.ToString() ?? "All");
    }

    private void SetActiveFilter(Button active)
    {
        foreach (var btn in new[] { FilterAll, FilterApplied, FilterAccepted, FilterRejected })
        {
            if (btn == active)
            {
                btn.Background = ActiveBg;
                btn.Foreground = ActiveFg;
                btn.BorderBrush = ActiveBdr;
            }
            else
            {
                btn.Background = InactiveBg;
                btn.Foreground = InactiveFg;
                btn.BorderBrush = InactiveBdr;
            }
        }
    }

    private void ViewJobDetails_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is Button { Tag: ApplicationCardModel model })
            Frame.Navigate(typeof(UserStatusJobDetailPage), model);
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs eventArguments)
        => viewModel.Refresh();

    private void GoToRecommendationsButton_Click(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(UserRecommendationPage));
}
