using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class SkillTestServiceTests
{
    private readonly FakeSkillTestRepository repo = new();
    private readonly SkillTestService service;

    public SkillTestServiceTests()
    {
        service = new SkillTestService(repo);
    }

    [Fact]
    public async Task GetTestsForUserAsync_returns_user_tests()
    {
        repo.Seed(
            new SkillTestBuilder().WithId(1).ForUser(1).Build(),
            new SkillTestBuilder().WithId(2).ForUser(2).Build());

        var result = await service.GetTestsForUserAsync(1);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(1);
    }

    [Fact]
    public async Task CanRetakeTestAsync_returns_true_when_test_older_than_eligibility_window()
    {
        repo.Seed(new SkillTestBuilder()
            .WithId(1)
            .WithAchievedDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(-4)))
            .Build());

        (await service.CanRetakeTestAsync(1)).Should().BeTrue();
    }

    [Fact]
    public async Task CanRetakeTestAsync_returns_false_when_test_inside_eligibility_window()
    {
        repo.Seed(new SkillTestBuilder()
            .WithId(1)
            .WithAchievedDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)))
            .Build());

        (await service.CanRetakeTestAsync(1)).Should().BeFalse();
    }

    [Fact]
    public async Task CanRetakeTestAsync_throws_when_test_missing()
    {
        Func<Task> act = () => service.CanRetakeTestAsync(404);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*No test found*");
    }

    [Fact]
    public async Task SubmitRetakeAsync_updates_score_and_date_and_returns_badge()
    {
        repo.Seed(new SkillTestBuilder()
            .WithId(1)
            .WithScore(40)
            .WithAchievedDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)))
            .Build());

        var badge = await service.SubmitRetakeAsync(1, newScore: 95);

        var test = await repo.GetByIdAsync(1);
        test!.Score.Should().Be(95);
        test.AchievedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));
        badge.Tier.Should().Be(BadgeTier.Gold);
        badge.ExperiencePointsValue.Should().Be(SimpleModelOperations.GoldExperiencePoints);
    }

    [Fact]
    public async Task SubmitRetakeAsync_throws_when_not_eligible()
    {
        repo.Seed(new SkillTestBuilder()
            .WithId(1)
            .WithAchievedDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-10)))
            .Build());

        Func<Task> act = () => service.SubmitRetakeAsync(1, 95);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*not yet eligible*");
    }

    [Fact]
    public async Task GetSkillTestByIdAsync_returns_test()
    {
        repo.Seed(new SkillTestBuilder().WithId(7).WithName("Algorithms").Build());

        (await service.GetSkillTestByIdAsync(7))!.Name.Should().Be("Algorithms");
    }

    [Fact]
    public void IsRetakeEligible_uses_3_month_window_static()
    {
        var oldEnough = new SkillTestBuilder()
            .WithAchievedDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(-4)))
            .Build();
        var tooRecent = new SkillTestBuilder()
            .WithAchievedDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-30)))
            .Build();

        SkillTestService.IsRetakeEligible(oldEnough).Should().BeTrue();
        SkillTestService.IsRetakeEligible(tooRecent).Should().BeFalse();
    }
}
