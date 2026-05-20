using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.CompletenessService;

public class CompletenessService : ICompletenessService
{
    private const int TotalFields = 21;

    private static readonly string[] Labels =
    {
        "First Name", "Last Name", "Age", "Gender", "Country",
        "Phone Number", "Email", "University", "Graduation Year", "GitHub",
        "LinkedIn", "Address", "Profile Picture", "Skills", "Motivation",
        "Work Experience", "Projects", "Activities", "Preferred Roles",
        "Work Mode", "Location Preference"
    };

    private bool IsFieldFilled(int index, User user)
    {
        switch (index)
        {
            case 0: return !string.IsNullOrWhiteSpace(user.FirstName);
            case 1: return !string.IsNullOrWhiteSpace(user.LastName);
            case 2: return user.Age > 0;
            case 3: return !string.IsNullOrWhiteSpace(user.Gender);
            case 4: return !string.IsNullOrWhiteSpace(user.Country);
            case 5: return !string.IsNullOrWhiteSpace(user.Phone);
            case 6: return !string.IsNullOrWhiteSpace(user.Email);
            case 7: return !string.IsNullOrWhiteSpace(user.University);
            case 8: return user.ExpectedGraduationYear > 0;
            case 9: return !string.IsNullOrWhiteSpace(user.GitHub);
            case 10: return !string.IsNullOrWhiteSpace(user.LinkedIn);
            case 11: return !string.IsNullOrWhiteSpace(user.Address);
            case 12: return !string.IsNullOrWhiteSpace(user.ProfilePicturePath);
            case 13: return user.Skills != null && user.Skills.Count > 0;
            case 14: return !string.IsNullOrWhiteSpace(user.Motivation);
            case 15: return user.WorkExperiences != null && user.WorkExperiences.Count > 0;
            case 16: return user.Projects != null && user.Projects.Count > 0;
            case 17: return user.ExtraCurricularActivities != null && user.ExtraCurricularActivities.Count > 0;
            // deviation: original UserProfile.PreferredJobRoles (list) has no direct equivalent on User;
            // PersonalityResult.SelectedRole is the closest — a role chosen after the personality test.
            case 18: return user.PersonalityResult?.SelectedRole != null;
            case 19: return !string.IsNullOrWhiteSpace(user.WorkModePreference);
            case 20: return !string.IsNullOrWhiteSpace(user.LocationPreference);
            default: return false;
        }
    }

    private int CalculateCompletnessPercentage(int filledFields)
    {
        return (int)Math.Round((double)filledFields / TotalFields * 100);
    }

    private int CountFilledFields(User user)
    {
        int filledFields = 0;

        for (int fieldIndex = 0; fieldIndex < TotalFields; fieldIndex++)
        {
            if (IsFieldFilled(fieldIndex, user))
            {
                filledFields++;
            }
        }

        return filledFields;
    }

    public int CalculateCompleteness(User? user)
    {
        if (user == null)
        {
            return 0;
        }
        int filledFields = CountFilledFields(user);
        return CalculateCompletnessPercentage(filledFields);
    }

    public string GetNextEmptyFieldPrompt(User? user)
    {
        if (user == null)
        {
            return string.Empty;
        }

        int filledFields = CountFilledFields(user);

        for (int fieldIndex = 0; fieldIndex < TotalFields; fieldIndex++)
        {
            if (!IsFieldFilled(fieldIndex, user))
            {
                int nextPercentage = CalculateCompletnessPercentage(filledFields + 1);
                return $"Add your {Labels[fieldIndex]} to reach {nextPercentage}% completeness!";
            }
        }

        return "Your profile is 100% complete!";
    }
}
