using PussyCats.App.Dtos.TI;
using PussyCats.Library.Domain;

namespace PussyCats.App.Services.TI;

/// <summary>
/// Adapts PussyCats domain entities (returned by the library service proxies) into the
/// <c>Ti*Dto</c> shapes the TI WinUI views bind to. This is the only seam left after the
/// redundant <c>TiJobsService</c> was removed in favour of <see cref="Library.Services.Jobs.IJobService"/>
/// and <see cref="Library.Services.Skills.ISkillService"/>.
/// </summary>
public static class TiJobMapper
{
    public static TiJobPostingDto ToDto(Job job) => new()
    {
        JobId = job.JobId,
        CompanyId = job.CompanyId,
        Photo = job.Photo,
        JobTitle = job.JobTitle,
        IndustryField = job.IndustryField,
        JobType = job.JobType,
        ExperienceLevel = job.ExperienceLevel,
        StartDate = job.StartDate,
        EndDate = job.EndDate,
        JobDescription = job.JobDescription,
        JobLocation = job.JobLocation,
        AvailablePositions = job.AvailablePositions,
        PostedAt = job.PostedAt,
        Salary = job.Salary,
        AmountPayed = job.AmountPayed,
        Deadline = job.Deadline,
        JobSkills = job.RequiredSkills.Select(requiredSkill => ToDto(requiredSkill, job.JobId)).ToList(),
    };

    public static TiSkillDto ToDto(Skill skill) => new()
    {
        SkillId = skill.SkillId,
        SkillName = skill.Name,
    };

    // JobSkill.RequiredLevel is the same DB column TI reads as RequiredPercentage.
    private static TiJobSkillDto ToDto(JobSkill jobSkill, int jobId) => new()
    {
        SkillId = jobSkill.Skill?.SkillId ?? 0,
        JobId = jobId,
        RequiredPercentage = jobSkill.RequiredLevel,
        SkillDto = jobSkill.Skill is null ? null : ToDto(jobSkill.Skill),
    };
}
