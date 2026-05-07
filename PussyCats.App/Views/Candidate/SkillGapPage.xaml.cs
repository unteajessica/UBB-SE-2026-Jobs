using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Candidate;

public sealed partial class SkillGapPage : Page
{
    private readonly SkillGapViewModel viewModel;

    public SkillGapPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<SkillGapViewModel>();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void OnLoaded(object sender, RoutedEventArgs eventArguments)
        => await viewModel.LoadDataAsync();

    private void BackToStatus_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs eventArguments)
        => viewModel.Refresh();
}
