using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats_App.Views.Controls;

namespace PussyCats_App.Views.Candidate;

public sealed partial class TestDashboardPage : Page
{
    private readonly TestDashboardViewModel viewModel;

    public TestDashboardPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<TestDashboardViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        var user = eventArguments.Parameter as User;
        await viewModel.LoadTestsAsync(user);
        RenderCards();
    }

    private void RenderCards()
    {
        TestCardsContainer.Children.Clear();
        foreach (var cardViewModel in viewModel.TestCards)
        {
            TestCardsContainer.Children.Add(new SkillTestCardControl(cardViewModel));
        }
    }
}
