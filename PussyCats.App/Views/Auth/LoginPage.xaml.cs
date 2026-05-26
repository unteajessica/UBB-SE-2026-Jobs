using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PussyCats.App.ViewModels.Auth;

namespace PussyCats_App.Views.Auth;

public sealed partial class LoginPage : Page
{
    private readonly LoginViewModel viewModel;

    public LoginPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<LoginViewModel>();
        viewModel.LoginSucceeded += OnLoginSucceeded;
        DataContext = viewModel;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.Password = PasswordBox.Password;
    }

    private void OnLoginSucceeded()
    {
        if (App.MainAppWindow is MainWindow mainWindow)
        {
            mainWindow.ShowAuthenticatedShell();
        }
    }
}
