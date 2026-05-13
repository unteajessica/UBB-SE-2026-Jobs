using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats_App.Services.CompatibilityService;

namespace PussyCats.Tests.Services;

public class CompatibilityServiceTests
{
    private readonly FakeUserSkillRepository userSkillRepository = new();
    private readonly FakeSkillGroupRepository skillGroupRepository = new();
    private readonly FakeUserRepository userRepository = new();
    private readonly CompatibilityService service;

    public CompatibilityServiceTests()
    {
        service = new CompatibilityService(userSkillRepository, skillGroupRepository, userRepository);
    }

    [Fact]
    public async Task CalculateForRoleAsync_RoleHasNoGroups_ReturnsInvalidScore()
    {
        const int invalidScore = -1, userId = 1;
        userRepository.Seed(new UserBuilder().WithId(userId).Build());

        var expectedRoleResult = await service.CalculateForRoleAsync(userId, JobRole.BackendDeveloper);

        expectedRoleResult.MatchScore.Should().Be(invalidScore);
        expectedRoleResult.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateForRoleAsync_UserHasVerifiedSkills_ScoresAgainstVerifiedSkills()
    {
        const int userId = 1, skillId = 1, score = 80;
        const string skillName = "C#";
        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        userSkillRepository.Seed(new UserSkill
        {
            User = new User { UserId = userId },
            Skill = new Skill { SkillId = skillId, Name = skillName },
            Score = score,
            IsVerified = true,
            AchievedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        });
        skillGroupRepository.Seed(new SkillGroup
        {
            SkillGroupId = 1,
            GroupName = "Backend Languages",
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = skillId, Name = skillName } },
        });

        var expectedRoleResult = await service.CalculateForRoleAsync(1, JobRole.BackendDeveloper);

        expectedRoleResult.MatchScore.Should().Be(score);
    }

    [Fact]
    public async Task CalculateForRoleAsync_UserHasUnverifiedCvSkills_ConsidersUnverifiedSkills()
    {
        const int userId = 1;
        var user = new UserBuilder().WithId(userId).Build();
        // ParsedCv format: line 0 = name, line 1 = university, line 2 = comma-separated skill list
        user.ParsedCv = "Ada Lovelace\nCambridge\nC#, Python";
        userRepository.Seed(user);
        skillGroupRepository.Seed(new SkillGroup
        {
            SkillGroupId = 1,
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = 1, Name = "C#" } },
        });

        var expectedRoleResult = await service.CalculateForRoleAsync(userId, JobRole.BackendDeveloper);

        // unverified skill scores at 0.5 becomes 50 after normalization
        const int expectedScore = 50;
        expectedRoleResult.MatchScore.Should().Be(expectedScore);
    }

    [Fact]
    public async Task CalculateForRoleAsync_ManyGroupsExist_CapsSuggestionsAtThree()
    {
        const int userId = 1, numberOfGroups = 5;
        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        var groups = new List<SkillGroup>();
        for (int groupIndex = 1; groupIndex <= numberOfGroups; groupIndex++)
        {
            groups.Add(new SkillGroup
            {
                SkillGroupId = groupIndex,
                GroupName = $"Group {groupIndex}",
                Weight = 1,
                JobRole = JobRole.BackendDeveloper,
                Skills = new List<Skill> { new() { SkillId = groupIndex, Name = $"Skill{groupIndex}" } },
            });
        }
        skillGroupRepository.Seed(groups.ToArray());

        var result = await service.CalculateForRoleAsync(userId, JobRole.BackendDeveloper);
        const int cappedAmountOfSuggestions = 3;

        result.Suggestions.Count.Should().BeLessOrEqualTo(cappedAmountOfSuggestions);
    }

    [Fact]
    public async Task CalculateForRoleAsync_UserHasNoSkills_ReturnsZeroScore()
    {
        const int userId = 1;
        userRepository.Seed(new UserBuilder().WithId(userId).Build());
        skillGroupRepository.Seed(new SkillGroup
        {
            SkillGroupId = 1,
            GroupName = "Backend Languages",
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = 1, Name = "C#" } },
        });
        var expectedRoleResult = await service.CalculateForRoleAsync(userId, JobRole.BackendDeveloper);
        const int expectedScore = 0;
        expectedRoleResult.MatchScore.Should().Be(expectedScore);
    }

    [Fact]
    public async Task CalculateForRoleAsync_UserHasMatchingSkills_ReturnsExpectedScore()
    {
        const int userId = 1;
        const string firstSkillName = "C#", secondSkillName = "Docker";
        userRepository.Seed(new UserBuilder().WithId(userId).Build());

        UserSkill firstUserSkill = new UserSkill
        {
            User = new User { UserId = userId },
            Skill = new Skill { SkillId = 1, Name = firstSkillName },
            Score = 80,
            IsVerified = true,
            AchievedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        UserSkill secondUserSkill = new UserSkill
        {
            User = new User { UserId = userId },
            Skill = new Skill { SkillId = 2, Name = secondSkillName },
            Score = 60,
            IsVerified = true,
            AchievedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        
        userSkillRepository.Seed(firstUserSkill, secondUserSkill);

        SkillGroup firstSkillGroup = new SkillGroup
        {
            SkillGroupId = 1,
            GroupName = "Backend Languages",
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = 1, Name = firstSkillName } },
        };

        SkillGroup secondSkillGroup = new SkillGroup
        {
            SkillGroupId = 2,
            GroupName = "DevOps Tools",
            Weight = 1,
            JobRole = JobRole.BackendDeveloper,
            Skills = new List<Skill> { new() { SkillId = 2, Name = secondSkillName } },
        };

        skillGroupRepository.Seed(firstSkillGroup, secondSkillGroup);

        var expectedRoleResult = await service.CalculateForRoleAsync(userId, JobRole.BackendDeveloper);
        const int expectedScore = 70; // average of 80 and 60

        expectedRoleResult.MatchScore.Should().Be(expectedScore);
    }

    [Fact]
    public async Task CalculateAllAsync_Called_ReturnsOneResultPerRole()
    {
        userRepository.Seed(new UserBuilder().WithId(1).Build());

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

        int expectedNumberOfSuggestions = 1;
        service.GetSuggestions(roleResult).Should().HaveCount(expectedNumberOfSuggestions);
    }
}
