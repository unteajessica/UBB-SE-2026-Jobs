using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TI;

public sealed partial class TiMainTestPage : Page
{
    public TiMainTestViewModel ViewModel { get; }

    public TiMainTestPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TiMainTestViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadTestsAsync();
    }

    private void StartTest_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag != null)
        {
            int testId = Convert.ToInt32(button.Tag);
            var selected = ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
            if (selected != null) ViewModel.SelectedTest = selected;
            Frame.Navigate(typeof(TiTestPage), testId);
        }
    }

    private void Card_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Button button && button.Tag != null)
        {
            int testId = Convert.ToInt32(button.Tag);
            var card = ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
            if (card != null) card.IsHovered = true;
        }
    }

    private void Card_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Button button && button.Tag != null)
        {
            int testId = Convert.ToInt32(button.Tag);
            var card = ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
            if (card != null) card.IsHovered = false;
        }
    }
}
