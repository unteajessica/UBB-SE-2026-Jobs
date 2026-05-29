using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.ViewModels;

internal static class ViewModelSupport
{
    public const int DefaultUserId = 1;

    public static int ResolveUserId(SessionContext session)
    {
        return session.UserId > 0 ? session.UserId : DefaultUserId;
    }

    public static string FormatJobRole(JobRole role)
    {
        return role switch
        {
            JobRole.FrontendDeveloper => "Frontend Developer",
            JobRole.BackendDeveloper => "Backend Developer",
            JobRole.UiUxDesigner => "UI/UX Designer",
            JobRole.DevOpsEngineer => "DevOps Engineer",
            JobRole.ProjectManager => "Project Manager",
            JobRole.DataAnalyst => "Data Analyst",
            JobRole.CybersecuritySpecialist => "Cybersecurity Specialist",
            JobRole.AiMlEngineer => "AI/ML Engineer",
            _ => role.ToString(),
        };
    }

    public static string BuildFreshnessLabel(DateTime lastUpdated)
    {
        if (lastUpdated == default)
        {
            return "Not updated yet";
        }

        var elapsed = DateTime.UtcNow - lastUpdated.ToUniversalTime();
        if (elapsed.TotalMinutes < 1)
        {
            return "Updated just now";
        }

        if (elapsed.TotalHours < 1)
        {
            return $"Updated {(int)elapsed.TotalMinutes} minute(s) ago";
        }

        if (elapsed.TotalDays < 1)
        {
            return $"Updated {(int)elapsed.TotalHours} hour(s) ago";
        }

        return $"Updated {(int)elapsed.TotalDays} day(s) ago";
    }

    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
        {
            return "***@***";
        }

        return email[0] + new string('*', atIndex - 1) + email[atIndex..];
    }

    public static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4)
        {
            return "***";
        }

        var visiblePrefixLength = Math.Min(2, phone.Length);
        var hiddenLength = Math.Max(0, phone.Length - visiblePrefixLength - 3);
        return phone[..visiblePrefixLength] + new string('*', hiddenLength) + phone[^3..];
    }

    public static IReadOnlyList<SkillDisplay> BuildSkillDisplay(IReadOnlyList<UserSkill> skills, int? takeCount)
    {
        var ordered = skills
            .OrderByDescending(skill => skill.Score)
            .ToList();

        var limit = takeCount ?? ordered.Count;
        return ordered
            .Take(limit)
            .Select(skill => new SkillDisplay
            {
                Name = skill.Skill?.Name ?? $"Skill {skill.Skill?.SkillId ?? 0}",
                Score = skill.Score,
            })
            .ToList();
    }
}

public sealed class FilterCheckItem : DispatchableObservableObject
{
    private bool isChecked;

    public FilterCheckItem(string label)
    {
        Label = label;
    }

    public string Label { get; }

    public bool IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }
}

public sealed class SkillFilterItem : DispatchableObservableObject
{
    private bool isChecked;

    public SkillFilterItem(int skillId, string name)
    {
        SkillId = skillId;
        Name = name;
    }

    public int SkillId { get; }
    public string Name { get; }

    public bool IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }
}

public sealed class SkillDisplay
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
}
