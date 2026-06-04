using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiSubmittedAnswersPage : Page
{
    public TiSubmittedAnswersViewModel ViewModel { get; }

    public TiSubmittedAnswersPage()
    {
        ViewModel = App.Services.GetRequiredService<TiSubmittedAnswersViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TiSubmittedAnswersParams p)
            await ViewModel.LoadAsync(p.TestId, p.AttemptId);
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();
}
