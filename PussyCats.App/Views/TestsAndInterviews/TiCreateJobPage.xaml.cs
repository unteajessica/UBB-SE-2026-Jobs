using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiCreateJobPage : Page
{
    public TiCreateJobViewModel ViewModel { get; }

    public TiCreateJobPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiCreateJobViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }

    private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox combo) return;
        var value = (combo.SelectedItem as ComboBoxItem)?.Tag as string ?? string.Empty;

        if (combo == IndustryCombo) ViewModel.IndustryField = value;
        else if (combo == JobTypeCombo) ViewModel.JobType = value;
        else if (combo == ExpLevelCombo) ViewModel.ExperienceLevel = value;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveJobAsync();
        if (ViewModel.SavedSuccessfully)
            Frame.Navigate(typeof(TiJobsPage));
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
}
