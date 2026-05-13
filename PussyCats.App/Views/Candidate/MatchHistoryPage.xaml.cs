using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Candidate;

public sealed partial class MatchHistoryPage : Page
{
    private readonly MatchHistoryViewModel viewModel;

    public MatchHistoryPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<MatchHistoryViewModel>();
        Loaded += OnLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void OnLoaded(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.LoadMatchesAsync();
        await viewModel.LoadStatisticsAsync();

        MatchesListView.ItemsSource = viewModel.GetMatches()
            .Select(match => new
            {
                CompanyName = match.Job?.Company?.CompanyName ?? "Unknown Company",
                JobRole     = match.Job is not null ? ViewModelSupport.FormatJobRole(match.Job.JobRole) : string.Empty,
                MatchDate   = match.Timestamp.ToString("dd MMM yyyy"),
            }).ToList();

        var stats = viewModel.GetStatistics();
        if (stats is not null)
        {
            totalMatchesLabel.Text          = $"Total Matches: {stats.TotalMatches}";
            matchesLastMonthLabel.Text      = $"Last Month: {stats.MatchesLastMonth}";
            matchesLastSixMonthsLabel.Text  = $"Last 6 Months: {stats.MatchesLastSixMonths}";
            matchesLastYearLabel.Text       = $"Last Year: {stats.MatchesLastYear}";

            PositionStatsListView.ItemsSource = stats.MatchesPerPosition
                .Select(keyValuePair => new { JobRole = keyValuePair.Key, Count = keyValuePair.Value })
                .ToList();
        }
    }
}
