using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats_App.Views.Controls;
using PussyCats_App.Views.TestsAndInterviews;

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

        if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
        {
            TestCardsContainer.Children.Add(new TextBlock
            {
                Text = viewModel.ErrorMessage,
                Foreground = new SolidColorBrush(Colors.White),
                TextWrapping = TextWrapping.Wrap,
            });
            return;
        }

        foreach (var cardViewModel in viewModel.TestCards)
        {
            var card = new SkillTestCardControl(cardViewModel);
            card.TakeTestRequested += OnTakeTestRequested;
            TestCardsContainer.Children.Add(card);
        }
    }

    private void OnTakeTestRequested(object? sender, int testId)
    {
        Frame.Navigate(typeof(TiTestPage), testId);
    }
}
