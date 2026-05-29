using System.Net;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.Services.Auth;

namespace PussyCats.App.ViewModels.Auth;

public partial class RegisterViewModel : DispatchableObservableObject
{
    private readonly IAuthService authService;
    private readonly SessionContext session;

    private string firstName = string.Empty;
    private string lastName = string.Empty;
    private string email = string.Empty;
    private string password = string.Empty;
    private string confirmPassword = string.Empty;
    private string errorMessage = string.Empty;
    private bool isBusy;

    public RegisterViewModel(IAuthService authService, SessionContext session)
    {
        this.authService = authService;
        this.session = session;
    }

    public event Action? RegisterSucceeded;

    public string FirstName
    {
        get => firstName;
        set => SetProperty(ref firstName, value);
    }

    public string LastName
    {
        get => lastName;
        set => SetProperty(ref lastName, value);
    }

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

    public string ConfirmPassword
    {
        get => confirmPassword;
        set => SetProperty(ref confirmPassword, value);
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
    private async Task RegisterAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
            return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName)
            || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please fill in all fields.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await authService.RegisterAsync(
                Email.Trim(),
                Password,
                FirstName.Trim(),
                LastName.Trim(),
                cancellationToken);

            if (!result.Succeeded || result.Response is null)
            {
                ErrorMessage = result.StatusCode == HttpStatusCode.Conflict
                    ? "This email address is already registered."
                    : "Registration failed. Please try again.";
                return;
            }

            session.SignIn(result.Response);
            RegisterSucceeded?.Invoke();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Registration failed: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
