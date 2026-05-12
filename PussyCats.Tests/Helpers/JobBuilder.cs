using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Tests.Helpers;

public class JobBuilder
{
    private int jobId = 1;
    private int companyId = 1;
    private string jobTitle = "Software Engineer";
    private string jobDescription = "Build things.";
    private string location = "Cluj-Napoca, Romania";
    private string employmentType = "Full-time";
    private int promotionLevel;
    private JobRole jobRole = JobRole.BackendDeveloper;
    private readonly List<JobSkill> requiredSkills = new();

    public JobBuilder WithId(int id)
    {
        jobId = id;
        return this;
    }

    public JobBuilder WithCompanyId(int id)
    {
        companyId = id;
        return this;
    }

    public JobBuilder WithTitle(string value)
    {
        jobTitle = value;
        return this;
    }

    public JobBuilder WithLocation(string value)
    {
        location = value;
        return this;
    }

    public JobBuilder WithEmploymentType(string value)
    {
        employmentType = value;
        return this;
    }

    public JobBuilder WithRole(JobRole value)
    {
        jobRole = value;
        return this;
    }

    public JobBuilder WithPromotionLevel(int value)
    {
        promotionLevel = value;
        return this;
    }

    public JobBuilder WithRequiredSkills(params (int skillId, int requiredLevel)[] entries)
    {
        foreach (var (skillId, level) in entries)
        {
            requiredSkills.Add(new JobSkill
            {
                Job = new Job { JobId = jobId },
                Skill = new Skill { SkillId = skillId },
                RequiredLevel = level,
            });
        }
        return this;
    }

    public Job Build()
    {
        foreach (var skill in requiredSkills)
        {
            skill.Job = new Job { JobId = jobId };
        }
        return new Job
        {
            JobId = jobId,
            Company = new Company { CompanyId = companyId },
            JobTitle = jobTitle,
            JobDescription = jobDescription,
            Location = location,
            EmploymentType = employmentType,
            PromotionLevel = promotionLevel,
            JobRole = jobRole,
            RequiredSkills = requiredSkills,
        };
    }
}
