using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Developer;

public sealed partial class DeveloperPage : Page
{
    public DeveloperPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<DeveloperViewModel>();
    }
}
