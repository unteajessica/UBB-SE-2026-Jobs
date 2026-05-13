using System.Text.Json;
using System.Text.RegularExpressions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

namespace PussyCats_App.Services.CvParsingService;

public class CvParsingService : ICvParsingService
{
    private const string JsonExtension = ".json";
    private const string ParseErrorMessage = "Failed to parse CV file: ";
    private const string UnsupportedTypeMessage = "Unsupported file type. Only JSON is supported.";

    private const int MaxSkills = 30;
    private const int MaxSkillLength = 60;

    private const int MaxFirstNameLength = 50;
    private const int MaxLastNameLength = 60;
    private const int MaxCountryLength = 100;
    private const int MaxCityLength = 100;
    private const int MaxUniversityLength = 200;
    private const int MaxGitHubLength = 200;
    private const int MaxLinkedInLength = 200;
    private const int MaxAddressLength = 500;
    private const int MaxMotivationLength = 1000;
    private const int MaxCompanyNameLength = 150;
    private const int MaxJobTitleLength = 100;
    private const int MaxWorkDescriptionLength = 500;

    private const int InvalidAge = 0;

    private const string GenderMale = "Male";
    private const string GenderFemale = "Female";
    private const string GenderMaleShort = "M";
    private const string GenderFemaleShort = "F";

    private static readonly DateTime MinValidDate = new DateTime(1980, 1, 1);
    private const int MaxYearsAheadForDate = 1;

    public User ParseCvFile(string content, string fileType)
    {
        try
        {
            if (string.Equals(fileType, JsonExtension, StringComparison.OrdinalIgnoreCase))
            {
                var cvData = JsonSerializer.Deserialize<CvData>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return MapCvDataToUser(cvData);
            }

            throw new Exception(UnsupportedTypeMessage);
        }
        catch (Exception exception)
        {
            throw new Exception(ParseErrorMessage + exception.Message, exception);
        }
    }

    private User MapCvDataToUser(CvData? cvData)
    {
        if (cvData == null)
        {
            return new User();
        }

        return new User
        {
            FirstName = SanitizeString(cvData.FirstName, MaxFirstNameLength),
            LastName = SanitizeString(cvData.LastName, MaxLastNameLength),
            Age = ValidateAge(cvData.Age),
            Gender = ValidateGender(cvData.Gender),
            Email = SanitizeEmail(cvData.Email),
            Phone = FormatPhoneNumber(cvData.PhoneNumber),
            Country = SanitizeString(cvData.Country, MaxCountryLength),
            City = SanitizeString(cvData.City, MaxCityLength),
            University = SanitizeString(cvData.University, MaxUniversityLength),
            ExpectedGraduationYear = ValidateGraduationYear(cvData.ExpectedGraduationYear),
            GitHub = SanitizeString(cvData.GitHub, MaxGitHubLength),
            LinkedIn = SanitizeString(cvData.LinkedIn, MaxLinkedInLength),
            Address = SanitizeString(cvData.Address, MaxAddressLength),
            Motivation = SanitizeString(cvData.Motivation, MaxMotivationLength),
            HasDisabilities = cvData.HasDisabilities,
            Skills = ProcessSkills(cvData.Skills),
            WorkExperiences = ProcessWorkExperiences(cvData.WorkExperiences),
            Projects = ProcessProjects(cvData.Projects),
            ExtraCurricularActivities = ProcessActivities(cvData.ExtraCurricularActivities)
        };
    }

    private List<UserSkill> ProcessSkills(List<string>? skills)
    {
        var result = new List<UserSkill>();

        if (skills == null)
        {
            return result;
        }

        var addedSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int addedSkillsCount = 0;

        foreach (var skill in skills)
        {
            if (addedSkillsCount >= MaxSkills)
            {
                break;
            }

            var sanitizedSkillName = SanitizeString(skill, MaxSkillLength);

            if (!string.IsNullOrWhiteSpace(sanitizedSkillName) && addedSkills.Add(sanitizedSkillName))
            {
                result.Add(new UserSkill
                {
                    Skill = new Skill { Name = sanitizedSkillName },
                    IsVerified = false,
                    Score = 0,
                });
                addedSkillsCount++;
            }
        }

        return result;
    }

    private List<WorkExperience> ProcessWorkExperiences(List<CvWorkExperience>? experiences)
    {
        if (experiences == null || !experiences.Any())
        {
            return new List<WorkExperience>();
        }
        const int maximumNumberOfWorkExperiences = 10;

        return experiences.Take(maximumNumberOfWorkExperiences)
            .Select(rawWorkExperience => new WorkExperience
            {
                Company = SanitizeString(rawWorkExperience.Company, MaxCompanyNameLength),
                JobTitle = SanitizeString(rawWorkExperience.JobTitle, MaxJobTitleLength),
                StartDate = ValidateDate(rawWorkExperience.StartDate),
                EndDate = rawWorkExperience.CurrentlyWorking ? null : ValidateDate(rawWorkExperience.EndDate),
                CurrentlyWorking = rawWorkExperience.CurrentlyWorking,
                Description = SanitizeString(rawWorkExperience.Description, MaxWorkDescriptionLength)
            })
            .Where(validatedWorkExperience => !string.IsNullOrEmpty(validatedWorkExperience.Company) && !string.IsNullOrEmpty(validatedWorkExperience.JobTitle))
            .ToList();
    }

    private List<Project> ProcessProjects(List<CvProject>? projects)
    {
        if (projects == null || !projects.Any())
        {
            return new List<Project>();
        }
        const int maximumNumberOfProjects = 10;
        const int maximumNumberOfTechnologiesPerProject = 10;
        const int maximumProjectNameLength = 100;
        const int maximumProjectDescriptionLength = 600;
        const int maximumProjectUrlLength = 200;
        const int maximumTechnologyNameLength = 60;

        return projects.Take(maximumNumberOfProjects)
            .Select(project => new Project
            {
                Name = SanitizeString(project.Name, maximumProjectNameLength),
                Description = SanitizeString(project.Description, maximumProjectDescriptionLength),
                Technologies = project.Technologies?.Take(maximumNumberOfTechnologiesPerProject).Select(technology => SanitizeString(technology, maximumTechnologyNameLength)).ToList() ?? new List<string>(),
                Url = SanitizeString(project.Url, maximumProjectUrlLength)
            })
            .Where(validProject => !string.IsNullOrEmpty(validProject.Name))
            .ToList();
    }

    private List<ExtraCurricularActivity> ProcessActivities(List<CvActivity>? activities)
    {
        if (activities == null || !activities.Any())
        {
            return new List<ExtraCurricularActivity>();
        }

        const int maximumNumberOfActivities = 10;
        const int maximumActivityNameLength = 150;
        const int maximumOrganizationNameLength = 100;
        const int maximumRoleNameLength = 80;
        const int maximumPeriodLength = 60;
        const int maximumActivityDescriptionLength = 300;

        return activities.Take(maximumNumberOfActivities)
            .Select(activity => new ExtraCurricularActivity
            {
                ActivityName = SanitizeString(activity.ActivityName, maximumActivityNameLength),
                Organization = SanitizeString(activity.Organization, maximumOrganizationNameLength),
                Role = SanitizeString(activity.Role, maximumRoleNameLength),
                Period = SanitizeString(activity.Period, maximumPeriodLength),
                Description = SanitizeString(activity.Description, maximumActivityDescriptionLength)
            })
            .Where(validActivity => !string.IsNullOrEmpty(validActivity.ActivityName))
            .ToList();
    }

    private string SanitizeString(string? inputToSanitize, int maximumAllowedLength)
    {
        if (string.IsNullOrWhiteSpace(inputToSanitize))
        {
            return string.Empty;
        }

        inputToSanitize = inputToSanitize.Trim();

        if (inputToSanitize.Length > maximumAllowedLength)
        {
            inputToSanitize = inputToSanitize.Substring(0, maximumAllowedLength);
        }

        return inputToSanitize;
    }

    private string SanitizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }
        const int maximumEmailLength = 254;

        email = email.Trim().ToLowerInvariant();

        if (email.Length > maximumEmailLength)
        {
            return string.Empty;
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            return string.Empty;
        }

        return email;
    }

    private int ValidateAge(int age)
    {
        const int minimumAge = 16;
        const int maximumAge = 60;

        if (age < minimumAge || age > maximumAge)
        {
            return InvalidAge;
        }
        return age;
    }

    private string ValidateGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
        {
            return string.Empty;
        }

        gender = gender.Trim();

        if (gender.Equals(GenderMale, StringComparison.OrdinalIgnoreCase) ||
            gender.Equals(GenderMaleShort, StringComparison.OrdinalIgnoreCase))
        {
            return GenderMale;
        }

        if (gender.Equals(GenderFemale, StringComparison.OrdinalIgnoreCase) ||
            gender.Equals(GenderFemaleShort, StringComparison.OrdinalIgnoreCase))
        {
            return GenderFemale;
        }

        return string.Empty;
    }

    private string FormatPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }
        const int maximumPhoneLength = 15;

        phoneNumber = phoneNumber.Trim();
        phoneNumber = Regex.Replace(phoneNumber, @"[^\d+]", string.Empty);

        if (phoneNumber.Length > maximumPhoneLength)
        {
            return string.Empty;
        }

        return phoneNumber;
    }

    private int ValidateGraduationYear(int year)
    {
        int currentYear = DateTime.Now.Year;
        const int maximumYearsAheadForGraduation = 10;

        if (year < currentYear || year > currentYear + maximumYearsAheadForGraduation)
        {
            return 0;
        }

        return year;
    }

    private DateTimeOffset ValidateDate(DateTimeOffset? date)
    {
        if (!date.HasValue)
        {
            return DateTimeOffset.Now;
        }

        var earliestAllowedDate = new DateTimeOffset(MinValidDate);
        var latestAllowedDate = DateTimeOffset.Now.AddYears(MaxYearsAheadForDate);

        if (date < earliestAllowedDate || date > latestAllowedDate)
        {
            return DateTimeOffset.Now;
        }
        return date.Value;
    }
}
