using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    [ObservableProperty] private string selectedDateLabel = string.Empty;
    [ObservableProperty] private bool noBookings = true;
    [ObservableProperty] private bool noAvailableSlots = true;

    public ObservableCollection<TiSlotDto> AvailableSlots { get; } = new();
    public ObservableCollection<TiSlotDto> MyBookings { get; } = new();
    public HashSet<DateTime> BookedDates { get; } = new();

    public TiInterviewSlotsViewModel(ITiSlotsService slotsService, SessionContext session)
    {
        this.slotsService = slotsService;
        this.session = session;
    }

    partial void OnSelectedDateChanged(DateTimeOffset value)
    {
        SelectedDateLabel = value.ToString("dddd, dd MMM yyyy");
        _ = LoadAvailableSlotsAsync();
    }

    public async Task LoadSlotsAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        SelectedDateLabel = SelectedDate.ToString("dddd, dd MMM yyyy");
        try
        {
            var availableTask = slotsService.GetAvailableAsync(SelectedDate.DateTime);
            var bookingsTask = slotsService.GetMyBookingsAsync(session.UserId);
            await System.Threading.Tasks.Task.WhenAll(availableTask, bookingsTask);

            var available = availableTask.Result;
            var bookings = bookingsTask.Result;

            await UIDispatcher.EnqueueAsync(() =>
            {
                AvailableSlots.Clear();
                foreach (var s in available.Where(s => s.Status == 0))
                    AvailableSlots.Add(s);
                NoAvailableSlots = AvailableSlots.Count == 0;

                MyBookings.Clear();
                BookedDates.Clear();
                foreach (var s in bookings)
                {
                    MyBookings.Add(s);
                    BookedDates.Add(s.StartTime.Date);
                }
                NoBookings = MyBookings.Count == 0;
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load slots: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAvailableSlotsAsync()
    {
        try
        {
            var slots = await slotsService.GetAvailableAsync(SelectedDate.DateTime);
            await UIDispatcher.EnqueueAsync(() =>
            {
                AvailableSlots.Clear();
                foreach (var s in slots.Where(s => s.Status == 0))
                    AvailableSlots.Add(s);
                NoAvailableSlots = AvailableSlots.Count == 0;
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load slots: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task BookSlotAsync(TiSlotDto slot)
    {
        try
        {
            bool success = await slotsService.BookSlotAsync(slot.Id, session.UserId);
            StatusMessage = success ? "Slot booked successfully!" : "Failed to book slot. Please try again.";
            if (success) await LoadSlotsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Booking failed: {ex.Message}";
        }
    }
}
