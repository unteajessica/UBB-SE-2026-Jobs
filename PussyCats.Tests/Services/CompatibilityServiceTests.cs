using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class CompatibilityServiceTests
{
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly FakeSkillGroupRepository skillGroupRepo = new();
    private readonly FakeUserRepository userRepo = new();
    private readonly CompatibilityService service;

    public CompatibilityServiceTests()
    {
        service = new CompatibilityService(userSkillRepo, skillGroupRepo, userRepo);
    }

    [Fact]
    public async Task CalculateForRoleAsync_returns_invalid_score_when_role_has_no_groups()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        var result = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        result.JobRole.Should().Be(JobRole.BackendDeveloper);
        result.MatchScore.Should().Be(-1);
        result.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateForRoleAsync_scores_against_user_verified_skills()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        userSkillRepo.Seed(new UserSkill
        {
            UserId = 1,
            SkillId = 1,
            Score = 80,
            IsVerified = true,
            AchievedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Skill = new Skill { SkillId = 1, Name = "C#" },
        });
        skillGroupRepo.Seed(new SkillGroup
        {
            SkillGroupId = 1,
            GroupName = "Backend Languages",
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = 1, Name = "C#" } },
        });

        var result = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        result.MatchScore.Should().Be(80);
    }

    [Fact]
    public async Task CalculateForRoleAsync_considers_unverified_cv_skills()
    {
        var user = new UserBuilder().WithId(1).Build();
        // ParsedCv format: line 0 = name, line 1 = university, line 2 = comma-separated skill list
        user.ParsedCv = "Ada Lovelace\nCambridge\nC#, Python";
        userRepo.Seed(user);
        skillGroupRepo.Seed(new SkillGroup
        {
            SkillGroupId = 1,
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = 1, Name = "C#" } },
        });

        var result = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        // unverified skill scores at 0.5 -> 50 after normalization
        result.MatchScore.Should().Be(50);
    }

    [Fact]
    public async Task CalculateForRoleAsync_caps_suggestions_at_three()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        var groups = new List<SkillGroup>();
        for (int i = 1; i <= 5; i++)
        {
            groups.Add(new SkillGroup
            {
                SkillGroupId = i,
                GroupName = $"Group {i}",
                Weight = 1,
                JobRole = JobRole.BackendDeveloper,
                Skills = new List<Skill> { new() { SkillId = i, Name = $"Skill{i}" } },
            });
        }
        skillGroupRepo.Seed(groups.ToArray());

        var result = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        result.Suggestions.Count.Should().BeLessOrEqualTo(3);
    }

    [Fact]
    public async Task CalculateAllAsync_returns_one_result_per_role()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        var results = await service.CalculateAllAsync(1);

        results.Should().HaveCount(Enum.GetValues<JobRole>().Length);
    }

    [Fact]
    public void GetSuggestions_returns_role_result_suggestions()
    {
        var roleResult = new PussyCats.Library.DTOs.RoleResult
        {
            JobRole = JobRole.BackendDeveloper,
            Suggestions = new List<PussyCats.Library.DTOs.Suggestion>
            {
                new() { SkillName = "Docker", GroupName = "DevOps", GainScore = 10 },
            },
        };

        service.GetSuggestions(roleResult).Should().HaveCount(1);
    }
}
