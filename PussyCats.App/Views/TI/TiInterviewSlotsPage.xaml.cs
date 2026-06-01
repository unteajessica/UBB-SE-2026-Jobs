using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TI;

public sealed partial class TiInterviewSlotsPage : Page
{
    public TiInterviewSlotsViewModel ViewModel { get; }

    public TiInterviewSlotsPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiInterviewSlotsViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadSlotsAsync();
    }

    private async void Filter_Click(object sender, RoutedEventArgs e)
        => await ViewModel.LoadSlotsAsync();

    private async void BookSlot_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TiSlotDto slot })
            await ViewModel.BookSlotAsync(slot);
    }
}
