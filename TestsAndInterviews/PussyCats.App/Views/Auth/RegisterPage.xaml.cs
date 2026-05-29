using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PussyCats.App.ViewModels.Auth;

namespace PussyCats_App.Views.Auth;

public sealed partial class RegisterPage : Page
{
    private readonly RegisterViewModel viewModel;

    public RegisterPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<RegisterViewModel>();
        viewModel.RegisterSucceeded += OnRegisterSucceeded;
        DataContext = viewModel;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.Password = PasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
    }

    private void NavigateToLogin_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (App.MainAppWindow is MainWindow mainWindow)
        {
            mainWindow.ShowLogin();
        }
    }

    private void OnRegisterSucceeded()
    {
        if (App.MainAppWindow is MainWindow mainWindow)
        {
            mainWindow.ShowAuthenticatedShell();
        }
    }
}
