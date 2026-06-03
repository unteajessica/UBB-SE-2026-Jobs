using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats.Library.Domain.Enums;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiEventsPage : Page
{
    public TiEventsViewModel ViewModel { get; }

    public TiEventsPage()
    {
        ViewModel = App.Services.GetRequiredService<TiEventsViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var session = App.Services.GetRequiredService<SessionContext>();
        CreateEventButton.Visibility = session.Mode == AppMode.Company ? Visibility.Visible : Visibility.Collapsed;
        await ViewModel.LoadAsync();
    }

    private void CreateEvent_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(TiCreateEventPage));
    }

    private void EditEvent_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TiEventDto dto })
            Frame.Navigate(typeof(TiEditEventPage), dto);
    }

    private async void DeleteEvent_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TiEventDto dto })
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Event",
                Content = $"Delete '{dto.Title}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                await ViewModel.DeleteEventAsync(dto.Id);
        }
    }
}
