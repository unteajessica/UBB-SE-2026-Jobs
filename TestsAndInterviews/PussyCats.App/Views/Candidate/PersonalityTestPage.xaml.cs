using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Candidate;

public sealed partial class PersonalityTestPage : Page
{
    private readonly PersonalityTestViewModel viewModel;

    public PersonalityTestPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<PersonalityTestViewModel>();
        DataContext = viewModel;
    }
    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        await viewModel.LoadExistingResultAsync();
    }
}
