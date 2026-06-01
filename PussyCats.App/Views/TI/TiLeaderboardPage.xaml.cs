using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TI;

public sealed partial class TiLeaderboardPage : Page
{
    public TiLeaderboardViewModel ViewModel { get; }

    public TiLeaderboardPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiLeaderboardViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        int testId = e.Parameter is int id ? id : 0;
        await ViewModel.LoadAsync(testId);
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(TiMainTestPage));
    private void PrevPage_Click(object sender, RoutedEventArgs e) => ViewModel.GoToPrevPage();
    private void NextPage_Click(object sender, RoutedEventArgs e) => ViewModel.GoToNextPage();
}
