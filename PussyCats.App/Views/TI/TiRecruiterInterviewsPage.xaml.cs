using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TI;

public sealed partial class TiRecruiterInterviewsPage : Page
{
    public TiRecruiterInterviewsViewModel ViewModel { get; }

    public TiRecruiterInterviewsPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiRecruiterInterviewsViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadAllAsync();
    }

    private void Calendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
    {
        if (e.AddedDates.Count > 0)
            ViewModel.SelectedDate = new DateTimeOffset(e.AddedDates[0].DateTime);
    }
}
