using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiTestDetailsPage : Page
{
    public TiTestDetailsViewModel ViewModel { get; }

    private int loadedTestId;

    public TiTestDetailsPage()
    {
        ViewModel = App.Services.GetRequiredService<TiTestDetailsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TiTestCardViewModel card)
        {
            ViewModel.Load(card);
            loadedTestId = card.TestId;
            StartTestButton.Visibility = ViewModel.IsCompanyMode ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

    private void StartTest_Click(object sender, RoutedEventArgs e) =>
        Frame.Navigate(typeof(TiTestPage), loadedTestId);

    public string FormatDate(DateTime value) =>
        value == default ? "—" : value.ToString("dd-MM-yyyy HH:mm");
}
