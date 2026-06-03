using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiInterviewSlotsPage : Page
{
    public TiInterviewSlotsViewModel ViewModel { get; }

    public TiInterviewSlotsPage()
    {
        ViewModel = App.Services.GetRequiredService<TiInterviewSlotsViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadSlotsAsync();
        slotsCalendar.SetDisplayDate(DateTimeOffset.Now);
    }

    private void Calendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
    {
        if (e.AddedDates.Count > 0)
            ViewModel.SelectedDate = e.AddedDates[0];
    }

    private void Calendar_DayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
    {
        if (args.Phase == 0)
        {
            args.RegisterUpdateCallback(Calendar_DayItemChanging);
            return;
        }

        var date = args.Item.Date.Date;
        if (ViewModel.BookedDates.Contains(date))
        {
            args.Item.SetDensityColors(new[]
            {
                Windows.UI.Color.FromArgb(255, 132, 148, 255)
            });
        }
    }

    private async void BookSlot_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TiSlotDto slot })
            await ViewModel.BookSlotAsync(slot);
    }
}
