using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiInterviewSlotsViewModel : DispatchableObservableObject
{
    private readonly ITiSlotsService slotsService;
    private readonly SessionContext session;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private DateTimeOffset selectedDate = DateTimeOffset.Now;
    [ObservableProperty] private string statusMessage = string.Empty;

    public ObservableCollection<TiSlotDto> AvailableSlots { get; } = new();

    public TiInterviewSlotsViewModel(ITiSlotsService slotsService, SessionContext session)
    {
        this.slotsService = slotsService;
        this.session = session;
    }

    public async Task LoadSlotsAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        var slots = await slotsService.GetAvailableAsync(SelectedDate.DateTime);
        AvailableSlots.Clear();
        foreach (var s in slots.Where(s => s.Status == 0)) AvailableSlots.Add(s);
        IsLoading = false;
    }

    [RelayCommand]
    public async Task BookSlotAsync(TiSlotDto slot)
    {
        bool success = await slotsService.BookSlotAsync(slot.Id, session.UserId);
        StatusMessage = success ? "Slot booked successfully!" : "Failed to book slot. Please try again.";
        if (success) await LoadSlotsAsync();
    }
}
