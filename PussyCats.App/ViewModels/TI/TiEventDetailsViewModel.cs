using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.ViewModels.TI;

public partial class TiEventDetailsViewModel : DispatchableObservableObject
{
    private readonly SessionContext session;

    [ObservableProperty] private TiEventDto? currentEvent;
    [ObservableProperty] private bool isCompanyMode;

    public TiEventDetailsViewModel(SessionContext session)
    {
        this.session = session;
    }

    public void Load(TiEventDto evt)
    {
        CurrentEvent = evt;
        IsCompanyMode = session.Mode == AppMode.Company;
    }
}
