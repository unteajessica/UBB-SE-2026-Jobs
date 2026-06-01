using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TI;

public sealed partial class TiJobApplicantsPage : Page
{
    public TiApplicantsViewModel ViewModel { get; }

    public TiJobApplicantsPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiApplicantsViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TiJobPostingDto job)
            await ViewModel.LoadForJobAsync(job);
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
}
