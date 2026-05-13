using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats_App.Services.CvParsingService;
using PussyCats_App.Services.UserProfileService;

namespace PussyCats.App.ViewModels;

public class ProfileFormViewModel : DispatchableObservableObject
{
    private const int MaximumNumberOfExtraCurricularActivitiesAllowed = 10;
    private const int MaximumNumberOfSkillsAllowed = 30;
    private const int MaximumSkillNameLength = 60;
    private const int MaximumNumberOfWorkExperiencesAllowed = 10;
    private const int MaximumNumberOfProjectsAllowed = 10;
    private const int MissingAgeDefaultValue = 0;
    private const int MissingGraduationYearDefaultValue = 0;

    private readonly IUserProfileService profileService;
    private readonly ICvParsingService cvParsingService;
    private readonly SessionContext session;
    private User userProfile = new();

    private string firstName = string.Empty;
    private string lastName = string.Empty;
    private double age;
    private string gender = string.Empty;
    private string email = string.Empty;
    private string phonePrefix = string.Empty;
    private string phoneNumber = string.Empty;
    private string gitHub = string.Empty;
    private string linkedIn = string.Empty;
    private string country = string.Empty;
    private string city = string.Empty;
    private string university = string.Empty;
    private string degree = string.Empty;
    private string address = string.Empty;
    private int universityStartYear;
    private int expectedGraduationYear;
    private string motivation = string.Empty;
    private bool hasDisabilities;
    private string errorMessage = string.Empty;
    private string cvStatusText = string.Empty;
    private string infoBarMessage = string.Empty;
    private InfoBarSeverity infoBarSeverity = InfoBarSeverity.Informational;
    private bool isInfoBarOpen;

    public ProfileFormViewModel(
        IUserProfileService profileService,
        ICvParsingService cvParsingService,
        SessionContext session)
    {
        this.profileService = profileService;
        this.cvParsingService = cvParsingService;
        this.session = session;

        var currentYear = DateTime.Now.Year;
        for (var year = currentYear; year <= currentYear + 10; year++)
        {
            GraduationYears.Add(year);
        }
    }

    public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }
    public string LastName { get => lastName; set => SetProperty(ref lastName, value); }
    public double Age { get => age; set => SetProperty(ref age, value); }
    public string Gender { get => gender; set => SetProperty(ref gender, value); }
    public string Email { get => email; set => SetProperty(ref email, value); }
    public string PhonePrefix { get => phonePrefix; set => SetProperty(ref phonePrefix, value); }
    public string PhoneNumber { get => phoneNumber; set => SetProperty(ref phoneNumber, value); }
    public string GitHub { get => gitHub; set => SetProperty(ref gitHub, value); }
    public string LinkedIn { get => linkedIn; set => SetProperty(ref linkedIn, value); }
    public string Country { get => country; set => SetProperty(ref country, value); }
    public string City { get => city; set => SetProperty(ref city, value); }
    public string University { get => university; set => SetProperty(ref university, value); }
    public string Degree { get => degree; set => SetProperty(ref degree, value); }
    public string Address { get => address; set => SetProperty(ref address, value); }
    public int UniversityStartYear { get => universityStartYear; set => SetProperty(ref universityStartYear, value); }
    public int ExpectedGraduationYear { get => expectedGraduationYear; set => SetProperty(ref expectedGraduationYear, value); }
    public string Motivation { get => motivation; set => SetProperty(ref motivation, value); }
    public bool HasDisabilities { get => hasDisabilities; set => SetProperty(ref hasDisabilities, value); }
    public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }
    public string CvStatusText { get => cvStatusText; set => SetProperty(ref cvStatusText, value); }
    public string InfoBarMessage { get => infoBarMessage; set => SetProperty(ref infoBarMessage, value); }
    public InfoBarSeverity InfoBarSeverity { get => infoBarSeverity; set => SetProperty(ref infoBarSeverity, value); }
    public bool IsInfoBarOpen { get => isInfoBarOpen; set => SetProperty(ref isInfoBarOpen, value); }

    public ObservableCollection<string> Skills { get; } = new();
    public ObservableCollection<WorkExperience> WorkExperiences { get; } = new();
    public ObservableCollection<Project> Projects { get; } = new();
    public ObservableCollection<ExtraCurricularActivity> ExtraCurricularActivities { get; } = new();
    public List<int> GraduationYears { get; } = new();

    public void LoadProfile(User? profile)
    {
        userProfile = profile ?? new User { UserId = ViewModelSupport.ResolveUserId(session) };

        FirstName = userProfile.FirstName;
        LastName = userProfile.LastName;
        Age = userProfile.Age;
        Gender = userProfile.Gender;
        Email = userProfile.Email;
        GitHub = userProfile.GitHub;
        LinkedIn = userProfile.LinkedIn;
        University = userProfile.University;
        Degree = userProfile.Degree;
        Address = userProfile.Address;
        Motivation = userProfile.Motivation;
        Country = userProfile.Country;
        City = userProfile.City;
        UniversityStartYear = userProfile.UniversityStartYear;
        ExpectedGraduationYear = userProfile.ExpectedGraduationYear;
        HasDisabilities = userProfile.HasDisabilities;

        var phoneParts = ExtractPhonePrefixAndNumber(userProfile.Phone);
        PhonePrefix = phoneParts.Prefix;
        PhoneNumber = phoneParts.Number;

        Skills.Clear();
        foreach (var skill in userProfile.Skills.Select(skill => skill.Skill?.Name).Where(name => !string.IsNullOrWhiteSpace(name)))
        {
            Skills.Add(skill!);
        }

        ReplaceCollection(WorkExperiences, userProfile.WorkExperiences);
        ReplaceCollection(Projects, userProfile.Projects);
        ReplaceCollection(ExtraCurricularActivities, userProfile.ExtraCurricularActivities);
    }

    public void AddSkill(string skill)
    {
        if (string.IsNullOrWhiteSpace(skill))
        {
            return;
        }

        skill = skill.Trim();
        if (IsDuplicateSkill(skill))
        {
            ShowInfoBar("This skill has already been added.", InfoBarSeverity.Warning);
            return;
        }

        if (Skills.Count >= MaximumNumberOfSkillsAllowed)
        {
            ShowInfoBar($"Maximum of {MaximumNumberOfSkillsAllowed} skills allowed.", InfoBarSeverity.Warning);
            return;
        }

        if (skill.Length > MaximumSkillNameLength)
        {
            ShowInfoBar($"Skill name must be less than {MaximumSkillNameLength} characters.", InfoBarSeverity.Warning);
            return;
        }

        Skills.Add(skill);
    }

    public void RemoveSkill(string skill) => Skills.Remove(skill);

    public void AddWorkExperience()
    {
        if (WorkExperiences.Count >= MaximumNumberOfWorkExperiencesAllowed)
        {
            ShowInfoBar($"Maximum of {MaximumNumberOfWorkExperiencesAllowed} work experiences allowed.", InfoBarSeverity.Warning);
            return;
        }

        WorkExperiences.Add(new WorkExperience { StartDate = DateTimeOffset.Now });
    }

    public void RemoveWorkExperience(WorkExperience workExperience) => WorkExperiences.Remove(workExperience);

    public void AddProject()
    {
        if (Projects.Count >= MaximumNumberOfProjectsAllowed)
        {
            ShowInfoBar($"Maximum of {MaximumNumberOfProjectsAllowed} projects allowed.", InfoBarSeverity.Warning);
            return;
        }

        Projects.Add(new Project());
    }

    public void RemoveProject(Project project) => Projects.Remove(project);

    public void AddExtraCurricularActivity()
    {
        if (ExtraCurricularActivities.Count >= MaximumNumberOfExtraCurricularActivitiesAllowed)
        {
            ShowInfoBar($"Maximum of {MaximumNumberOfExtraCurricularActivitiesAllowed} extra-curricular activities allowed.", InfoBarSeverity.Warning);
            return;
        }

        ExtraCurricularActivities.Add(new ExtraCurricularActivity());
    }

    public void RemoveExtraCurricularActivity(ExtraCurricularActivity activity) => ExtraCurricularActivities.Remove(activity);

    public async Task<bool> SaveProfileAsync(CancellationToken cancellationToken = default)
    {
        var errors = ValidateForm();
        if (errors.Count > 0)
        {
            ShowInfoBar($"Please fill in required fields: {string.Join(", ", errors)}", InfoBarSeverity.Error);
            return false;
        }

        UpdateProfileFromForm();

        try
        {
            await profileService.SaveAsync(userProfile.UserId, userProfile, cancellationToken);
            ShowInfoBar("Profile saved successfully!", InfoBarSeverity.Success);
            return true;
        }
        catch (Exception exception)
        {
            ShowInfoBar($"Error saving profile: {exception.Message}", InfoBarSeverity.Error);
            return false;
        }
    }

    public User GetUpdatedProfile()
    {
        UpdateProfileFromForm();
        return userProfile;
    }

    public void ProcessCvFile(string content, string fileType)
    {
        try
        {
            var parsedUser = cvParsingService.ParseCvFile(content, fileType);
            PopulateFromParsedProfile(parsedUser);
            CvStatusText = "CV loaded successfully!";
            ShowInfoBar("CV data has been loaded. Please review and complete any missing fields.", InfoBarSeverity.Success);
        }
        catch (Exception exception)
        {
            ShowInfoBar($"Error processing CV file: {exception.InnerException?.Message ?? exception.Message}", InfoBarSeverity.Error);
        }
    }

    public void PopulateFromParsedProfile(User parsedUser)
    {
        if (!string.IsNullOrEmpty(parsedUser.FirstName)) FirstName = parsedUser.FirstName;
        if (!string.IsNullOrEmpty(parsedUser.LastName)) LastName = parsedUser.LastName;
        if (parsedUser.Age > MissingAgeDefaultValue) Age = parsedUser.Age;
        if (!string.IsNullOrEmpty(parsedUser.Gender)) Gender = parsedUser.Gender;
        if (!string.IsNullOrEmpty(parsedUser.Email)) Email = parsedUser.Email;
        if (!string.IsNullOrEmpty(parsedUser.Phone))
        {
            var parts = ExtractPhonePrefixAndNumber(parsedUser.Phone);
            PhonePrefix = parts.Prefix;
            PhoneNumber = parts.Number;
        }

        if (!string.IsNullOrEmpty(parsedUser.GitHub)) GitHub = parsedUser.GitHub;
        if (!string.IsNullOrEmpty(parsedUser.LinkedIn)) LinkedIn = parsedUser.LinkedIn;
        if (!string.IsNullOrEmpty(parsedUser.Country)) Country = parsedUser.Country;
        if (!string.IsNullOrEmpty(parsedUser.City)) City = parsedUser.City;
        if (!string.IsNullOrEmpty(parsedUser.University)) University = parsedUser.University;
        if (!string.IsNullOrEmpty(parsedUser.Degree)) Degree = parsedUser.Degree;
        if (parsedUser.UniversityStartYear > 0) UniversityStartYear = parsedUser.UniversityStartYear;
        if (parsedUser.ExpectedGraduationYear > MissingGraduationYearDefaultValue) ExpectedGraduationYear = parsedUser.ExpectedGraduationYear;
        if (!string.IsNullOrEmpty(parsedUser.Address)) Address = parsedUser.Address;
        if (!string.IsNullOrEmpty(parsedUser.Motivation)) Motivation = parsedUser.Motivation;

        Skills.Clear();
        WorkExperiences.Clear();
        Projects.Clear();
        ExtraCurricularActivities.Clear();

        foreach (var skillName in parsedUser.Skills.Select(skill => skill.Skill?.Name).Where(name => !string.IsNullOrWhiteSpace(name)))
        {
            if (!IsDuplicateSkill(skillName!) && Skills.Count < MaximumNumberOfSkillsAllowed)
            {
                Skills.Add(skillName!);
            }
        }

        foreach (var workExperience in parsedUser.WorkExperiences.Take(MaximumNumberOfWorkExperiencesAllowed))
        {
            WorkExperiences.Add(workExperience);
        }

        foreach (var project in parsedUser.Projects.Take(MaximumNumberOfProjectsAllowed))
        {
            Projects.Add(project);
        }

        foreach (var activity in parsedUser.ExtraCurricularActivities.Take(MaximumNumberOfExtraCurricularActivitiesAllowed))
        {
            ExtraCurricularActivities.Add(activity);
        }

        var missingFields = ValidateForm();
        if (missingFields.Count > 0)
        {
            ShowInfoBar($"Missing fields: {string.Join(", ", missingFields)}", InfoBarSeverity.Warning);
        }
    }

    public List<string> FilterUniversities(string universityQuery)
    {
        var options = new[]
        {
            "Babes-Bolyai University",
            "University of Bucharest",
            "Technical University of Cluj-Napoca",
            "Politehnica University of Bucharest",
        };

        return options
            .Where(universityName => universityName.Contains(universityQuery ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<string> FilterSkillSuggestions(string searchTextQuery)
    {
        if (string.IsNullOrWhiteSpace(searchTextQuery))
        {
            return new List<string>();
        }

        var suggestions = new[]
        {
            "C#", ".NET", "SQL", "JavaScript", "TypeScript", "React", "Angular", "Azure",
            "Docker", "Git", "Python", "UI/UX", "Machine Learning", "Cybersecurity",
        };

        return suggestions
            .Where(skill => skill.Contains(searchTextQuery, StringComparison.OrdinalIgnoreCase) && !IsDuplicateSkill(skill))
            .ToList();
    }

    public bool IsDuplicateSkill(string skill)
    {
        return Skills.Any(existingSkill => existingSkill.Equals(skill, StringComparison.OrdinalIgnoreCase));
    }

    public bool SkillMatchesSearchAndIsNotDuplicate(string skill, string searchText)
    {
        return skill.Contains(searchText, StringComparison.OrdinalIgnoreCase) && !IsDuplicateSkill(skill);
    }

    private void UpdateProfileFromForm()
    {
        userProfile.UserId = userProfile.UserId > 0 ? userProfile.UserId : ViewModelSupport.ResolveUserId(session);
        userProfile.FirstName = FirstName.Trim();
        userProfile.LastName = LastName.Trim();
        userProfile.Age = (int)Age;
        userProfile.Gender = Gender;
        userProfile.Email = Email.Trim().ToLowerInvariant();
        userProfile.Phone = PhonePrefix + PhoneNumber.Trim();
        userProfile.GitHub = GitHub.Trim();
        userProfile.LinkedIn = LinkedIn.Trim();
        userProfile.Country = Country.Trim();
        userProfile.City = City.Trim();
        userProfile.University = University.Trim();
        userProfile.Degree = Degree.Trim();
        userProfile.UniversityStartYear = UniversityStartYear;
        userProfile.ExpectedGraduationYear = ExpectedGraduationYear;
        userProfile.Address = Address.Trim();
        userProfile.Motivation = Motivation.Trim();
        userProfile.HasDisabilities = HasDisabilities;
        userProfile.Skills = Skills.Select(skill => new UserSkill
        {
            User = new User { UserId = userProfile.UserId },
            Skill = new Skill { Name = skill },
            Score = 0,
            IsVerified = false,
        }).ToList();
        userProfile.WorkExperiences = WorkExperiences.ToList();
        userProfile.Projects = Projects.ToList();
        userProfile.ExtraCurricularActivities = ExtraCurricularActivities.ToList();
        userProfile.LastUpdated = DateTime.UtcNow;
    }

    private List<string> ValidateForm()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(FirstName)) errors.Add("First Name");
        if (string.IsNullOrWhiteSpace(LastName)) errors.Add("Last Name");
        if (Age <= MissingAgeDefaultValue) errors.Add("Age");
        if (string.IsNullOrWhiteSpace(Gender)) errors.Add("Gender");
        if (string.IsNullOrWhiteSpace(Email)) errors.Add("Email");
        if (string.IsNullOrWhiteSpace(PhonePrefix) && string.IsNullOrWhiteSpace(PhoneNumber)) errors.Add("Phone Number");
        if (string.IsNullOrWhiteSpace(Country)) errors.Add("Country");
        if (string.IsNullOrWhiteSpace(City)) errors.Add("City");
        if (string.IsNullOrWhiteSpace(University)) errors.Add("University");
        if (ExpectedGraduationYear <= MissingGraduationYearDefaultValue) errors.Add("Expected Graduation Year");
        return errors;
    }

    private void ShowInfoBar(string message, InfoBarSeverity severity)
    {
        InfoBarMessage = message;
        InfoBarSeverity = severity;
        IsInfoBarOpen = true;
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private static (string Prefix, string Number) ExtractPhonePrefixAndNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = phone.Trim();
        if (!trimmed.StartsWith('+'))
        {
            return (string.Empty, trimmed);
        }

        var splitIndex = trimmed.IndexOfAny([' ', '-'], 1);
        if (splitIndex <= 0)
        {
            return (string.Empty, trimmed);
        }

        return (trimmed[..splitIndex], trimmed[(splitIndex + 1)..]);
    }

    public async Task LoadCurrentUserAsync()
    {
        int userId = ViewModelSupport.ResolveUserId(session);
        var currentUser = await profileService.GetProfileAsync(userId);

        if (currentUser != null)
        {
            LoadProfile(currentUser);
        }
    }
}
