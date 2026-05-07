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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await viewModel.LoadMatchesAsync();
        await viewModel.LoadStatisticsAsync();

        MatchesListView.ItemsSource = viewModel.GetMatches()
            .Select(m => new
            {
                CompanyName = m.Job?.Company?.CompanyName ?? $"Job {m.JobId}",
                JobRole     = m.Job is not null ? ViewModelSupport.FormatJobRole(m.Job.JobRole) : m.JobId.ToString(),
                MatchDate   = m.Timestamp.ToString("dd MMM yyyy"),
            }).ToList();

        var stats = viewModel.GetStatistics();
        if (stats is not null)
        {
            totalMatchesLabel.Text          = $"Total Matches: {stats.TotalMatches}";
            matchesLastMonthLabel.Text      = $"Last Month: {stats.MatchesLastMonth}";
            matchesLastSixMonthsLabel.Text  = $"Last 6 Months: {stats.MatchesLastSixMonths}";
            matchesLastYearLabel.Text       = $"Last Year: {stats.MatchesLastYear}";

            PositionStatsListView.ItemsSource = stats.MatchesPerPosition
                .Select(kv => new { JobRole = kv.Key, Count = kv.Value })
                .ToList();
        }
    }
}
