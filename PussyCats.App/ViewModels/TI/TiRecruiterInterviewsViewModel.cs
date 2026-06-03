using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiRecruiterInterviewsViewModel : DispatchableObservableObject
{
    private readonly ITiSlotsService slotsService;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private DateTimeOffset selectedDate = DateTimeOffset.Now;
    [ObservableProperty] private string selectedDateFormatted = DateTimeOffset.Now.ToString("dddd, dd MMM yyyy");

    public ObservableCollection<TiSlotDto> Slots { get; } = new();
    public ObservableCollection<TiInterviewSessionDto> PendingReviews { get; } = new();

    public TiRecruiterInterviewsViewModel(ITiSlotsService slotsService)
    {
        this.slotsService = slotsService;
    }

    partial void OnSelectedDateChanged(DateTimeOffset value)
    {
        SelectedDateFormatted = value.ToString("dddd, dd MMM yyyy");
        _ = LoadSlotsAsync();
    }

    public async Task LoadAllAsync()
    {
        IsLoading = true;
        await LoadSlotsAsync();
        await LoadPendingReviewsAsync();
        IsLoading = false;
    }

    private async Task LoadSlotsAsync()
    {
        var slots = await slotsService.GetAvailableAsync(SelectedDate.DateTime);
        await UIDispatcher.EnqueueAsync(() =>
        {
            Slots.Clear();
            foreach (var s in slots) Slots.Add(s);
        });
    }

    private async Task LoadPendingReviewsAsync()
    {
        var sessions = await slotsService.GetSessionsByStatusAsync("InProgress");
        await UIDispatcher.EnqueueAsync(() =>
        {
            PendingReviews.Clear();
            foreach (var s in sessions) PendingReviews.Add(s);
        });
    }
}
