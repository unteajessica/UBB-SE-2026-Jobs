using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class SkillGapServiceTests
{
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeJobSkillRepository jobSkillRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly SkillGapService service;

    public SkillGapServiceTests()
    {
        service = new SkillGapService(
            matchRepo,
            new JobSkillService(jobSkillRepo),
            new UserSkillService(userSkillRepo));
    }

    [Fact]
    public async Task GetMissingSkillsAsync_NoRejectionsExist_ReturnsEmptyList()
    {
        matchRepo.Seed(new Match { MatchId = 1, User = new User { UserId = 1 }, JobId = 1, Status = MatchStatus.Applied });

        var result = await service.GetMissingSkillsAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMissingSkillsAsync_RejectionsExist_AggregatesSkillsUserLacksAcrossRejectedJobs()
    {
        matchRepo.Seed(
            new Match { MatchId = 1, User = new User { UserId = 1 }, JobId = 10, Status = MatchStatus.Rejected },
            new Match { MatchId = 2, User = new User { UserId = 1 }, JobId = 20, Status = MatchStatus.Rejected });
        userSkillRepo.Seed(new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 50 });
        jobSkillRepo.Seed(
            new JobSkill { Job = new Job { JobId = 10 }, Skill = new Skill { SkillId = 2, Name = "Docker" }, RequiredLevel = 70 },
            new JobSkill { Job = new Job { JobId = 10 }, Skill = new Skill { SkillId = 3, Name = "Kubernetes" }, RequiredLevel = 70 },
            new JobSkill { Job = new Job { JobId = 20 }, Skill = new Skill { SkillId = 2, Name = "Docker" }, RequiredLevel = 80 });

        var result = await service.GetMissingSkillsAsync(1);

        result.Should().HaveCount(2);
        result[0].SkillName.Should().Be("Docker");
        result[0].RejectedJobCount.Should().Be(2);
        result[1].SkillName.Should().Be("Kubernetes");
        result[1].RejectedJobCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUnderscoredSkillsAsync_UserMeetsRequiredLevel_SkipsThoseSkills()
    {
        matchRepo.Seed(new Match { MatchId = 1, User = new User { UserId = 1 }, JobId = 10, Status = MatchStatus.Rejected });
        userSkillRepo.Seed(new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 80 });
        jobSkillRepo.Seed(new JobSkill
        {
            Job = new Job { JobId = 10 },
            RequiredLevel = 50,
            Skill = new Skill { SkillId = 1, Name = "C#" },
        });

        var result = await service.GetUnderscoredSkillsAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnderscoredSkillsAsync_UserBelowRequiredLevel_ReturnsAverageRequiredScore()
    {
        matchRepo.Seed(
            new Match { MatchId = 1, User = new User { UserId = 1 }, JobId = 10, Status = MatchStatus.Rejected },
            new Match { MatchId = 2, User = new User { UserId = 1 }, JobId = 20, Status = MatchStatus.Rejected });
        userSkillRepo.Seed(new UserSkill { User = new User{ UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 30 });
        jobSkillRepo.Seed(
            new JobSkill { Job = new Job { JobId = 10 }, RequiredLevel = 70, Skill = new Skill { SkillId = 1, Name = "C#" } },
            new JobSkill { Job = new Job { JobId = 20 }, RequiredLevel = 90, Skill = new Skill { SkillId = 1, Name = "C#" } });

        var result = await service.GetUnderscoredSkillsAsync(1);

        result.Should().HaveCount(1);
        result[0].SkillName.Should().Be("C#");
        result[0].UserScore.Should().Be(30);
        result[0].AverageRequiredScore.Should().Be(80);
    }

    [Fact]
    public async Task GetSummaryAsync_UserHasNoRejections_ReportsNoRejections()
    {
        var summary = await service.GetSummaryAsync(1);

        summary.HasRejections.Should().BeFalse();
        summary.HasSkillGaps.Should().BeFalse();
    }

    [Fact]
    public async Task GetSummaryAsync_UserHasRejections_ReportsGapCounts()
    {
        matchRepo.Seed(new Match { MatchId = 1, User = new User { UserId = 1 }, JobId = 10, Status = MatchStatus.Rejected });
        userSkillRepo.Seed(new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 30 });
        jobSkillRepo.Seed(
            new JobSkill { Job = new Job { JobId = 10 }, Skill = new Skill { SkillId = 1, Name = "C#" }, RequiredLevel = 80 },
            new JobSkill { Job = new Job { JobId = 10 }, Skill = new Skill { SkillId = 2, Name = "SQL" }, RequiredLevel = 70 });

        var summary = await service.GetSummaryAsync(1);

        summary.HasRejections.Should().BeTrue();
        summary.HasSkillGaps.Should().BeTrue();
        summary.MissingSkillsCount.Should().Be(1);
        summary.SkillsToImproveCount.Should().Be(1);
    }
}