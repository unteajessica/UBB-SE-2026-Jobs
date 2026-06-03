using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiEventDetailsPage : Page
{
    public TiEventDetailsViewModel ViewModel { get; }

    private TiEventDto? currentDto;

    public TiEventDetailsPage()
    {
        ViewModel = App.Services.GetRequiredService<TiEventDetailsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TiEventDto dto)
        {
            currentDto = dto;
            ViewModel.Load(dto);
            CompanyActions.Visibility = ViewModel.IsCompanyMode ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

    private void EditEvent_Click(object sender, RoutedEventArgs e)
    {
        if (currentDto is not null)
            Frame.Navigate(typeof(TiEditEventPage), currentDto);
    }

    private async void DeleteEvent_Click(object sender, RoutedEventArgs e)
    {
        if (currentDto is null)
            return;

        var dialog = new ContentDialog
        {
            Title = "Delete Event",
            Content = $"Delete '{currentDto.Title}'?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot,
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            Frame.GoBack();
        }
    }

    public string FormatText(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value;

    public string FormatDate(DateTime value) => value == default ? "—" : value.ToString("dd MMM yyyy");
}
