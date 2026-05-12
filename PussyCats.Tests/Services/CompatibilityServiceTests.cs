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
    public async Task CalculateForRoleAsync_RoleHasNoGroups_ReturnsInvalidScore()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        var expectedRoleResult = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        expectedRoleResult.JobRole.Should().Be(JobRole.BackendDeveloper);
        expectedRoleResult.MatchScore.Should().Be(-1);
        expectedRoleResult.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateForRoleAsync_UserHasVerifiedSkills_ScoresAgainstVerifiedSkills()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());
        userSkillRepo.Seed(new UserSkill
        {
            User = new User { UserId = 1 },
            Skill = new Skill { SkillId = 1, Name = "C#" },
            Score = 80,
            IsVerified = true,
            AchievedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        });
        skillGroupRepo.Seed(new SkillGroup
        {
            SkillGroupId = 1,
            GroupName = "Backend Languages",
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = 1, Name = "C#" } },
        });

        var expectedRoleResult = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        expectedRoleResult.MatchScore.Should().Be(80);
    }

    [Fact]
    public async Task CalculateForRoleAsync_UserHasUnverifiedCvSkills_ConsidersUnverifiedSkills()
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

        var expectedRoleResult = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        // unverified skill scores at 0.5 -> 50 after normalization
        expectedRoleResult.MatchScore.Should().Be(50);
    }

    [Fact]
    public async Task CalculateForRoleAsync_ManyGroupsExist_CapsSuggestionsAtThree()
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
    public async Task CalculateAllAsync_Called_ReturnsOneResultPerRole()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        var results = await service.CalculateAllAsync(1);

        results.Should().HaveCount(Enum.GetValues<JobRole>().Length);
    }

    [Fact]
    public void GetSuggestions_RoleResultProvided_ReturnsRoleResultSuggestions()
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
