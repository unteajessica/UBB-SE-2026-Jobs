using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.Tests.Integration;

internal static class ViewModelTestData
{
    public static JobRecommendationResult JobCard(int jobId = 10, int recommendationId = 99)
    {
        return new JobRecommendationResult
        {
            Job = new Job
            {
                JobId = jobId,
                Company = new Company { CompanyId = 3 },
                JobTitle = "Backend Developer",
                JobDescription = "Build APIs.",
                Location = "Cluj-Napoca",
                EmploymentType = "Full-time",
                JobRole = JobRole.BackendDeveloper,
            },
            Company = new Company
            {
                CompanyId = 3,
                CompanyName = "Acme",
                Email = "hr@acme.test",
                Phone = "+40000000000",
            },
            CompatibilityScore = 87,
            DisplayRecommendationId = recommendationId,
        };
    }

    public static ApplicationCardModel Application(int matchId, MatchStatus status)
    {
        return new ApplicationCardModel
        {
            MatchId = matchId,
            JobId = 20 + matchId,
            CompanyName = "Acme",
            JobDescription = "Build things.",
            AppliedDate = new DateTime(2026, 5, 1),
            Status = status,
            CompatibilityScore = 75,
        };
    }

    public static UserApplicationResult Applicant(int matchId = 7, int companyId = 4, MatchStatus status = MatchStatus.Applied)
    {
        var user = new User
        {
            UserId = 12,
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Phone = "+40123456789",
        };

        var job = new Job
        {
            JobId = 30,
            Company = new Company { CompanyId = companyId },
            JobTitle = "Engineer",
            JobDescription = "Build software.",
            EmploymentType = "Full-time",
            Location = "Cluj-Napoca",
            JobRole = JobRole.BackendDeveloper,
        };

        var match = new Match
        {
            MatchId = matchId,
            JobId = job.JobId,
            Status = status,
            FeedbackMessage = "Good fit.",
            User = user,
            Job = job,
        };

        return new UserApplicationResult
        {
            User = user,
            Job = job,
            Match = match,
            CompatibilityScore = 82,
            UserSkills =
            [
                new UserSkill { User = new User { UserId = 1 }, Score = 80, Skill = new Skill { SkillId = 1, Name = "C#" } },
                new UserSkill { User = new User { UserId = 1 }, Score = 70, Skill = new Skill { SkillId = 2, Name = "SQL" } },
            ],
        };
    }

    public static SkillGapSummaryModel SkillGapSummary(bool hasRejections = true, bool hasSkillGaps = true)
    {
        return new SkillGapSummaryModel
        {
            HasRejections = hasRejections,
            HasSkillGaps = hasSkillGaps,
            MissingSkillsCount = hasSkillGaps ? 1 : 0,
            SkillsToImproveCount = hasSkillGaps ? 1 : 0,
        };
    }
}
