using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats_App.Services.MatchService;

namespace PussyCats.App.ViewModels;

public class MatchHistoryViewModel : DispatchableObservableObject
{
    private readonly IMatchService matchService;
    private readonly SessionContext session;
    private List<Match> matches = new();
    private MatchStatistics? statistics;
    private string errorMessage = string.Empty;

    public MatchHistoryViewModel(IMatchService matchService, SessionContext session)
    {
        this.matchService = matchService;
        this.session = session;
    }

    public List<Match> Matches
    {
        get => matches;
        private set => SetProperty(ref matches, value);
    }

    public MatchStatistics? Statistics
    {
        get => statistics;
        private set => SetProperty(ref statistics, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public async Task LoadMatchesAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;
        try
        {
            Matches = (await matchService
                .GetMatchesForUserAsync(ViewModelSupport.ResolveUserId(session), cancellationToken)
                ).ToList();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public async Task LoadStatisticsAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;
        try
        {
            Statistics = await matchService
                .GetMatchStatisticsAsync(ViewModelSupport.ResolveUserId(session), cancellationToken)
                ;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public List<Match> GetMatches() => Matches;
    public MatchStatistics? GetStatistics() => Statistics;
    public string GetErrorMessage() => ErrorMessage;
}
