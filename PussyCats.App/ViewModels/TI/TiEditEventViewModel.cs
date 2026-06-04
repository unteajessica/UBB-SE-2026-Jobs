using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiEditEventViewModel : DispatchableObservableObject
{
    private readonly ITiEventsService eventsService;
    private int eventId;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string location = string.Empty;
    [ObservableProperty] private DateTimeOffset? startDate;
    [ObservableProperty] private DateTimeOffset? endDate;
    [ObservableProperty] private string titleError = string.Empty;
    [ObservableProperty] private string dateError = string.Empty;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private bool updatedSuccessfully;
    [ObservableProperty] private bool deletedSuccessfully;

    public TiEditEventViewModel(ITiEventsService eventsService)
    {
        this.eventsService = eventsService;
    }

    public void LoadEvent(TiEventDto eventDto)
    {
        eventId = eventDto.Id;
        Title = eventDto.Title;
        Description = eventDto.Description;
        Location = eventDto.Location;
        StartDate = new DateTimeOffset(eventDto.StartDate);
        EndDate = new DateTimeOffset(eventDto.EndDate);
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title)) { TitleError = "Title is required."; return; }
        TitleError = string.Empty;

        IsSaving = true;
        var dto = new TiEventDto
        {
            Id = eventId,
            Title = Title.Trim(),
            Description = Description.Trim(),
            Location = Location.Trim(),
            StartDate = StartDate?.DateTime ?? DateTime.UtcNow,
            EndDate = EndDate?.DateTime ?? DateTime.UtcNow,
        };

        await eventsService.UpdateAsync(eventId, dto);
        IsSaving = false;
        UpdatedSuccessfully = true;
    }

    [RelayCommand]
    public async Task DeleteAsync()
    {
        await eventsService.DeleteAsync(eventId);
        DeletedSuccessfully = true;
    }
}
