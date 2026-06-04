using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;

namespace PussyCats.App.ViewModels.TI;

public partial class TiCreateEventViewModel : DispatchableObservableObject
{
    private readonly ITiEventsService eventsService;
    private readonly SessionContext session;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string location = string.Empty;
    [ObservableProperty] private DateTimeOffset? startDate;
    [ObservableProperty] private DateTimeOffset? endDate;
    [ObservableProperty] private string titleError = string.Empty;
    [ObservableProperty] private string descriptionError = string.Empty;
    [ObservableProperty] private string locationError = string.Empty;
    [ObservableProperty] private string dateError = string.Empty;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private bool createdSuccessfully;

    public TiCreateEventViewModel(ITiEventsService eventsService, SessionContext session)
    {
        this.eventsService = eventsService;
        this.session = session;
    }

    [RelayCommand]
    public async Task CreateEventAsync()
    {
        if (!Validate()) return;

        IsSaving = true;
        var dto = new TiEventDto
        {
            Title = Title.Trim(),
            Description = Description.Trim(),
            Location = Location.Trim(),
            StartDate = StartDate!.Value.DateTime,
            EndDate = EndDate!.Value.DateTime,
            HostCompanyId = session.CompanyId ?? 1,
            PostedAt = DateTime.UtcNow,
        };

        await eventsService.CreateAsync(dto);
        IsSaving = false;
        CreatedSuccessfully = true;
    }

    private bool Validate()
    {
        bool ok = true;
        TitleError = string.Empty;
        DescriptionError = string.Empty;
        LocationError = string.Empty;
        DateError = string.Empty;

        if (string.IsNullOrWhiteSpace(Title)) { TitleError = "Title is required."; ok = false; }
        if (string.IsNullOrWhiteSpace(Description)) { DescriptionError = "Description is required."; ok = false; }
        if (string.IsNullOrWhiteSpace(Location)) { LocationError = "Location is required."; ok = false; }
        if (StartDate == null) { DateError = "Start date is required."; ok = false; }
        if (EndDate == null) { DateError = "End date is required."; ok = false; }
        if (StartDate != null && EndDate != null && EndDate < StartDate)
        { DateError = "End date must be after start date."; ok = false; }
        return ok;
    }
}
