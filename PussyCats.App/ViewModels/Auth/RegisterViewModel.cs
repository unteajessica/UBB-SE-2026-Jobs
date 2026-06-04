using System.Collections.ObjectModel;
using System.Net;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Dtos.TI;
using PussyCats.App.Services.TI;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Auth;

namespace PussyCats.App.ViewModels.Auth;

public partial class RegisterViewModel : DispatchableObservableObject
{
    private const string CandidateRole = "Candidate";
    private const string RecruiterRole = "Recruiter";

    private readonly IAuthService authService;
    private readonly ITiAuthService tiAuthService;
    private readonly SessionContext session;

    private string firstName = string.Empty;
    private string lastName = string.Empty;
    private string email = string.Empty;
    private string password = string.Empty;
    private string confirmPassword = string.Empty;
    private string selectedRole = CandidateRole;
    private TiCompanyDto? selectedCompany;
    private string errorMessage = string.Empty;
    private bool isBusy;

    public RegisterViewModel(IAuthService authService, ITiAuthService tiAuthService, SessionContext session)
    {
        this.authService = authService;
        this.tiAuthService = tiAuthService;
        this.session = session;
    }

    public event Action? RegisterSucceeded;

    public IReadOnlyList<string> Roles { get; } = new[] { CandidateRole, RecruiterRole };

    public ObservableCollection<TiCompanyDto> Companies { get; } = new();

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

    public string SelectedRole
    {
        get => selectedRole;
        set
        {
            if (SetProperty(ref selectedRole, value))
            {
                OnPropertyChanged(nameof(IsRecruiter));
            }
        }
    }

    public bool IsRecruiter => string.Equals(SelectedRole, RecruiterRole, StringComparison.Ordinal);

    public TiCompanyDto? SelectedCompany
    {
        get => selectedCompany;
        set => SetProperty(ref selectedCompany, value);
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
    private async Task LoadCompaniesAsync()
    {
        if (Companies.Count > 0)
        {
            return;
        }

        var companies = await tiAuthService.GetCompaniesAsync();
        Companies.Clear();
        foreach (var company in companies)
        {
            Companies.Add(company);
        }
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

        if (IsRecruiter && SelectedCompany is null)
        {
            ErrorMessage = "Please select a company.";
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
                // If a recruiter already exists in PussyCats, the Recruiter record may
                // still be missing on the T&I side, so fall through via login instead
                // of blocking (mirrors the web registration flow).
                if (result.StatusCode == HttpStatusCode.Conflict && IsRecruiter)
                {
                    var loginResult = await authService.LoginAsync(Email.Trim(), Password, cancellationToken);
                    if (!loginResult.Succeeded || loginResult.Response is null)
                    {
                        ErrorMessage = "This email address is already registered.";
                        return;
                    }

                    result = loginResult;
                }
                else
                {
                    ErrorMessage = result.StatusCode == HttpStatusCode.Conflict
                        ? "This email address is already registered."
                        : "Registration failed. Please try again.";
                    return;
                }
            }

            if (IsRecruiter)
            {
                var name = $"{FirstName.Trim()} {LastName.Trim()}".Trim();
                var recruiterRegistered = await tiAuthService.RegisterAsync(
                    name,
                    Email.Trim(),
                    Password,
                    RecruiterRole,
                    SelectedCompany!.CompanyId);

                if (!recruiterRegistered)
                {
                    ErrorMessage = "Recruiter setup failed: company not found or already registered. Please try again.";
                    return;
                }
            }

            session.SignIn(result.Response);

            if (IsRecruiter)
            {
                session.Mode = AppMode.Company;
                session.CompanyId = SelectedCompany!.CompanyId;
            }

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
