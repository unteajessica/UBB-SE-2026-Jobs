using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiLeaderboardViewModel : DispatchableObservableObject
{
    private readonly ITiLeaderboardService leaderboardService;
    private readonly SessionContext session;
    private List<TiLeaderboardEntryDto> allEntries = new();
    private int currentPage = 1;
    private const int PageSize = 10;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private int testId;
    [ObservableProperty] private TiLeaderboardEntryDto? currentUserEntry;
    [ObservableProperty] private int totalPages = 1;
    [ObservableProperty] private bool canGoPrev;
    [ObservableProperty] private bool canGoNext;
    [ObservableProperty] private string pageInfo = "1 / 1";

    public ObservableCollection<TiLeaderboardEntryDto> TopEntries { get; } = new();
    public ObservableCollection<TiLeaderboardEntryDto> PageEntries { get; } = new();

    public TiLeaderboardViewModel(ITiLeaderboardService leaderboardService, SessionContext session)
    {
        this.leaderboardService = leaderboardService;
        this.session = session;
    }

    public async Task LoadAsync(int testId)
    {
        TestId = testId;
        IsLoading = true;

        allEntries = await leaderboardService.GetByTestIdAsync(testId);
        CurrentUserEntry = await leaderboardService.GetUserEntryAsync(testId, session.UserId);

        var top = allEntries.OrderBy(e => e.RankPosition).Take(3).ToList();
        TopEntries.Clear();
        foreach (var e in top) TopEntries.Add(e);

        TotalPages = Math.Max(1, (int)Math.Ceiling(allEntries.Count / (double)PageSize));
        currentPage = 1;
        UpdatePage();
        IsLoading = false;
    }

    public void GoToPrevPage()
    {
        if (currentPage > 1) { currentPage--; UpdatePage(); }
    }

    public void GoToNextPage()
    {
        if (currentPage < TotalPages) { currentPage++; UpdatePage(); }
    }

    private void UpdatePage()
    {
        var slice = allEntries.OrderBy(e => e.RankPosition).Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();
        PageEntries.Clear();
        foreach (var e in slice) PageEntries.Add(e);
        CanGoPrev = currentPage > 1;
        CanGoNext = currentPage < TotalPages;
        PageInfo = $"{currentPage} / {TotalPages}";
    }
}
