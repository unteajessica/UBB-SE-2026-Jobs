using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TI;

public sealed partial class TiPaymentPage : Page
{
    public TiPaymentViewModel ViewModel { get; }

    public TiPaymentPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiPaymentViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TiJobPostingDto job)
        {
            ViewModel.JobId = job.JobId;
            ViewModel.JobTitle = job.JobTitle ?? string.Empty;
        }

        await ViewModel.LoadDataAsync();
    }

    private async void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo)
        {
            var value = (combo.SelectedItem as ComboBoxItem)?.Tag as string ?? string.Empty;
            if (combo == JobTypeFilterCombo) ViewModel.SelectedJobType = value;
            else if (combo == ExpLevelFilterCombo) ViewModel.SelectedExperienceLevel = value;
            await ViewModel.LoadDataAsync();
        }
    }

    private async void Pay_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.PayAsync();
        if (ViewModel.PaymentSucceeded)
        {
            var dialog = new ContentDialog
            {
                Title = "Payment Successful",
                Content = "Your payment has been processed.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            await dialog.ShowAsync();
            Frame.Navigate(typeof(TiJobsPage));
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
}
