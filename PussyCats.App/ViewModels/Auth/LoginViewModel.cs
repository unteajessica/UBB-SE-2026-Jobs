using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.Services.Auth;

namespace PussyCats.App.ViewModels.Auth;

public partial class LoginViewModel : DispatchableObservableObject
{
    private readonly IAuthService authService;
    private readonly SessionContext session;

    private string email = string.Empty;
    private string password = string.Empty;
    private string errorMessage = string.Empty;
    private bool isBusy;

    public LoginViewModel(IAuthService authService, SessionContext session)
    {
        this.authService = authService;
        this.session = session;
    }

    public event Action? LoginSucceeded;

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    [RelayCommand]
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Enter your email and password.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await authService.LoginAsync(Email.Trim(), Password, cancellationToken);
            if (!result.Succeeded || result.Response is null)
            {
                ErrorMessage = "Invalid email or password.";
                return;
            }

            session.SignIn(result.Response);
            LoginSucceeded?.Invoke();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Login failed: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
