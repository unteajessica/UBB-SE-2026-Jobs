using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiEventsViewModel : DispatchableObservableObject
{
    private readonly ITiEventsService eventsService;
    private readonly SessionContext session;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool showPastEvents;

    public ObservableCollection<TiEventDto> CurrentEvents { get; } = new();
    public ObservableCollection<TiEventDto> PastEvents { get; } = new();

    public TiEventsViewModel(ITiEventsService eventsService, SessionContext session)
    {
        this.eventsService = eventsService;
        this.session = session;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        int companyId = session.CompanyId ?? 1;

        var current = await eventsService.GetCurrentEventsAsync(companyId);
        var past = await eventsService.GetPastEventsAsync(companyId);

        CurrentEvents.Clear();
        foreach (var e in current) CurrentEvents.Add(e);

        PastEvents.Clear();
        foreach (var e in past) PastEvents.Add(e);

        IsLoading = false;
    }

    public async Task DeleteEventAsync(int eventId)
    {
        await eventsService.DeleteAsync(eventId);
        await LoadAsync();
    }
}
