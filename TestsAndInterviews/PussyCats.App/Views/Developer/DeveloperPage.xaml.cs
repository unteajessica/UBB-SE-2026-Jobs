using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Developer;

public sealed partial class DeveloperPage : Page
{
    public DeveloperPage()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<DeveloperViewModel>();

        Loaded += DeveloperPage_Loaded;
    }

    private async void DeveloperPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DeveloperViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
