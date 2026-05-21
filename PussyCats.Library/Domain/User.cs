using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class User
{
    public int UserId { get; set; }

    public string Name => $"{FirstName} {LastName}".Trim();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string University { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int UniversityStartYear { get; set; }
    public int ExpectedGraduationYear { get; set; }

    public string GitHub { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;
    public bool HasDisabilities { get; set; }

    public string ProfilePicturePath { get; set; } = string.Empty;
    public string ParsedCv { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string PreferredEmploymentType { get; set; } = string.Empty;
    public string WorkModePreference { get; set; } = string.Empty;
    public string LocationPreference { get; set; } = string.Empty;

    public int YearsOfExperience { get; set; }
    public int TotalExperiencePoints { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public bool ActiveAccount { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    public List<WorkExperience> WorkExperiences { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public List<ExtraCurricularActivity> ExtraCurricularActivities { get; set; } = new();
    public List<UserSkill> Skills { get; set; } = new();
    public List<Match> Matches { get; set; } = new();

    public PersonalityTestResult? PersonalityResult { get; set; }   
}
