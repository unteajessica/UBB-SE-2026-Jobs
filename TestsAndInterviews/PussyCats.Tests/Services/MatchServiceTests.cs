using FluentAssertions;
using PussyCats.Library.Services;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.Users;

namespace PussyCats.Tests.Services;

public class MatchServiceTests
{
    private readonly FakeMatchRepository matchRepository = new();
    private readonly FakeJobRepository jobRepository = new();
    private readonly FakeUserRepository userRepository = new();
    private readonly MatchService service;

    public MatchServiceTests()
    {
        service = new MatchService(matchRepository, new JobService(jobRepository), new UserService(userRepository));
    }

    [Theory]
    [InlineData(MatchStatus.Applied, MatchStatus.Accepted, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Rejected, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Advanced, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Applied, false)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Accepted, true)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Rejected, true)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Advanced, false)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Applied, false)]
    [InlineData(MatchStatus.Accepted, MatchStatus.Rejected, false)]
    [InlineData(MatchStatus.Accepted, MatchStatus.Advanced, false)]
    [InlineData(MatchStatus.Accepted, MatchStatus.Applied, false)]
    [InlineData(MatchStatus.Rejected, MatchStatus.Accepted, false)]
    [InlineData(MatchStatus.Rejected, MatchStatus.Advanced, false)]
    [InlineData(MatchStatus.Rejected, MatchStatus.Applied, false)]
    public void IsDecisionTransitionAllowed_StateTransitionRequested_MatchesStateMachine(MatchStatus from, MatchStatus to, bool allowed)
    {
        var match = new MatchBuilder().WithStatus(from).Build();

        service.IsDecisionTransitionAllowed(match, to).Should().Be(allowed);
    }

    [Fact]
    public async Task CreatePendingApplicationAsync_ValidInputs_CreatesMatchInAppliedState()
    {
        const int userId = 1, jobId = 10;
        userRepository.Seed(new PussyCats.Library.Domain.User { UserId = userId });
        jobRepository.Seed(new JobBuilder().WithId(jobId).Build());
        var matchId = await service.CreatePendingApplicationAsync(userId, jobId);

        matchId.Should().BeGreaterThan(0);
        var match = await service.GetByIdAsync(matchId);

        match!.Status.Should().Be(MatchStatus.Applied);
        match.User.UserId.Should().Be(userId);
        match.Job.JobId.Should().Be(jobId);
    }

    [Fact]
    public async Task CreatePendingApplicationAsync_MatchAlreadyExists_ThrowsInvalidOperationException()
    {
        const int userId = 1, jobId = 10;
        matchRepository.Seed(new MatchBuilder().WithId(1).AppliedFor(userId, jobId).Build());

        Func<Task> act = () => service.CreatePendingApplicationAsync(userId, jobId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task SubmitDecisionAsync_StatusIsApplied_AcceptsMatch()
    {
        matchRepository.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());
        const string feedback = "Welcome";
        const int matchId = 1;

        await service.SubmitDecisionAsync(matchId, MatchStatus.Accepted, feedback);

        var match = await service.GetByIdAsync(matchId);
        match!.Status.Should().Be(MatchStatus.Accepted);
        match.FeedbackMessage.Should().Be(feedback);
    }

    [Fact]
    public async Task SubmitDecisionAsync_StatusIsAppliedAndFeedbackProvided_RejectsMatchWithFeedback()
    {
        const int matchId = 1;
        matchRepository.Seed(new MatchBuilder().WithId(matchId).WithStatus(MatchStatus.Applied).Build());

        await service.SubmitDecisionAsync(matchId, MatchStatus.Rejected, "Lacking experience");

        (await service.GetByIdAsync(matchId))!.Status.Should().Be(MatchStatus.Rejected);
    }

    [Fact]
    public async Task SubmitDecisionAsync_RejectingWithoutFeedback_ThrowsArgumentException()
    {
        matchRepository.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());

        Func<Task> act = () => service.SubmitDecisionAsync(1, MatchStatus.Rejected, string.Empty);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Feedback is required*");
    }

    [Fact]
    public async Task SubmitDecisionAsync_TargetStatusIsInvalid_ThrowsArgumentException()
    {
        const int matchId = 1;
        matchRepository.Seed(new MatchBuilder().WithId(matchId).WithStatus(MatchStatus.Applied).Build());

        Func<Task> act = () => service.SubmitDecisionAsync(matchId, MatchStatus.Applied, "x");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SubmitDecisionAsync_TransitionNotAllowed_ThrowsInvalidOperationException()
    {
        matchRepository.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Accepted).Build());

        Func<Task> act = () => service.SubmitDecisionAsync(1, MatchStatus.Rejected, "x");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SubmitDecisionAsync_MatchIsMissing_ThrowsKeyNotFoundException()
    {
        Func<Task> act = () => service.SubmitDecisionAsync(404, MatchStatus.Accepted, "x");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AdvanceAsync_StatusIsApplied_MovesToAdvanced()
    {
        matchRepository.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Applied).Build());

        await service.AdvanceAsync(1);

        (await service.GetByIdAsync(1))!.Status.Should().Be(MatchStatus.Advanced);
    }

    [Fact]
    public async Task AdvanceAsync_StatusIsNotApplied_ThrowsInvalidOperationException()
    {
        matchRepository.Seed(new MatchBuilder().WithId(1).WithStatus(MatchStatus.Advanced).Build());

        Func<Task> act = () => service.AdvanceAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RevertToAppliedAsync_Called_ResetsStatusAndFeedback()
    {
        matchRepository.Seed(new MatchBuilder()
            .WithId(1)
            .WithStatus(MatchStatus.Rejected)
            .WithFeedback("Sorry")
            .Build());

        await service.RevertToAppliedAsync(1);

        var match = await service.GetByIdAsync(1);
        match!.Status.Should().Be(MatchStatus.Applied);
        match.FeedbackMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCompanyIdAsync_CompanyHasJobs_FiltersViaJobsOwnedByCompany()
    {
        int firstJobId = 10, secondJobId = 20;
        int firstCompanyId = 5, secondCompanyId = 99;
        int numberOfHoursAgo = 2;
        int expectedNumberOfMatches = 2;
        const int firstUserId = 1, secondUserId = 2, thirdUserId = 3;
        jobRepository.Seed(
            new JobBuilder().WithId(firstJobId).WithCompanyId(firstCompanyId).Build(),
            new JobBuilder().WithId(secondJobId).WithCompanyId(secondCompanyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, firstJobId).WithTimestamp(DateTime.UtcNow.AddHours(-numberOfHoursAgo)).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, firstJobId).WithTimestamp(DateTime.UtcNow).Build(),
            new MatchBuilder().WithId(3).AppliedFor(thirdUserId, secondJobId).Build());

        var matches = await service.GetByCompanyIdAsync(firstCompanyId);

        matches.Should().HaveCount(expectedNumberOfMatches);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_CompanyHasJobs_ReturnsSortedDescendinglyByTime()
    {
        int firstJobId = 10, secondJobId = 20;
        int firstCompanyId = 5, secondCompanyId = 99;
        int numberOfHoursAgo = 2;
        int expectedFirstMatchId = 2, expectedSecondMatchId = 1;
        const int firstUserId = 1, secondUserId = 2, thirdUserId = 3;
        jobRepository.Seed(
            new JobBuilder().WithId(firstJobId).WithCompanyId(firstCompanyId).Build(),
            new JobBuilder().WithId(secondJobId).WithCompanyId(secondCompanyId).Build());
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, firstJobId).WithTimestamp(DateTime.UtcNow.AddHours(-numberOfHoursAgo)).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, firstJobId).WithTimestamp(DateTime.UtcNow).Build(),
            new MatchBuilder().WithId(3).AppliedFor(thirdUserId, secondJobId).Build());

        var matches = await service.GetByCompanyIdAsync(firstCompanyId);

        matches[0].MatchId.Should().Be(expectedFirstMatchId);
        matches[1].MatchId.Should().Be(expectedSecondMatchId);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_CompanyHasJobsButNoMatches_ReturnsEmpty()
    {
        int jobId = 10, companyId = 5;
        jobRepository.Seed(new JobBuilder().WithId(jobId).WithCompanyId(companyId).Build());
        var matches = await service.GetByCompanyIdAsync(companyId);
        matches.Should().BeEmpty();
    }
        
    [Fact]
    public async Task GetByCompanyIdAsync_CompanyHasBothJobsAndMatchesButNotMatching_ReturnsEmpty()
    {
        int firstJobId = 10, secondJobId = 20;
        int firstCompanyId = 5, secondCompanyId = 99;
        jobRepository.Seed(
            new JobBuilder().WithId(firstJobId).WithCompanyId(firstCompanyId).Build(),
            new JobBuilder().WithId(secondJobId).WithCompanyId(secondCompanyId).Build());
        matchRepository.Seed(new MatchBuilder().WithId(1).AppliedFor(1, secondJobId).Build());
        var matches = await service.GetByCompanyIdAsync(firstCompanyId);
        matches.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCompanyIdAsync_CompanyHasNoJobs_ReturnsEmpty()
    {
        int companyId = 99;
        var matches = await service.GetByCompanyIdAsync(companyId);

        matches.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMatchStatisticsAsync_MatchesExistAcrossTimeRanges_CountsWithinEachWindow()
    {
        const int userId = 1;
        const int firstJobId = 10, secondJobId = 11, thirdJobId = 12, fourthJobId = 13;
        const int firstJobDaysAgo = 10, secondJobMonthsAgo = 3, thirdJobMonthsAgo = 9, fourthJobYearsAgo = 2;
        var currentDate = DateTime.Now;
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(userId, firstJobId).WithTimestamp(currentDate.AddDays(-firstJobDaysAgo)).Build(),
            new MatchBuilder().WithId(2).AppliedFor(userId, secondJobId).WithTimestamp(currentDate.AddMonths(-secondJobMonthsAgo)).Build(),
            new MatchBuilder().WithId(3).AppliedFor(userId, thirdJobId).WithTimestamp(currentDate.AddMonths(-thirdJobMonthsAgo)).Build(),
            new MatchBuilder().WithId(4).AppliedFor(userId, fourthJobId).WithTimestamp(currentDate.AddYears(-fourthJobYearsAgo)).Build());

        var stats = await service.GetMatchStatisticsAsync(userId);

        const int expectedTotalMatches = 4;
        const int expectedMatchesLastMonth = 1, expectedMatchesLastSixMonths = 2, expectedMatchesLastYear = 3;
        stats.TotalMatches.Should().Be(expectedTotalMatches);
        stats.MatchesLastMonth.Should().Be(expectedMatchesLastMonth);
        stats.MatchesLastSixMonths.Should().Be(expectedMatchesLastSixMonths);
        stats.MatchesLastYear.Should().Be(expectedMatchesLastYear);
    }

    [Fact]
    public async Task GetMatchStatisticsAsync_MatchesExistForDifferentRoles_GroupsMatchesByPositionLabel()
    {
        const int userId = 1, firstJobId = 10, secondJobId = 11;
        var match1 = new MatchBuilder().WithId(1).AppliedFor(userId, firstJobId).Build();
        match1.Job = new JobBuilder().WithId(firstJobId).WithRole(JobRole.BackendDeveloper).Build();
        var match2 = new MatchBuilder().WithId(2).AppliedFor(userId, secondJobId).Build();
        match2.Job = new JobBuilder().WithId(secondJobId).WithRole(JobRole.FrontendDeveloper).Build();
        matchRepository.Seed(match1, match2);

        var stats = await service.GetMatchStatisticsAsync(userId);

        stats.MatchesPerPosition.Should().ContainKey("Backend");
        stats.MatchesPerPosition.Should().ContainKey("Frontend");
    }

    [Fact]
    public async Task GetMatchesForUserAsync_UserHasMatches_ReturnsUserMatches()
    {
        const int firstUserId = 1, secondUserId = 2;
        const int jobId = 10;
        matchRepository.Seed(
            new MatchBuilder().WithId(1).AppliedFor(firstUserId, jobId).Build(),
            new MatchBuilder().WithId(2).AppliedFor(secondUserId, jobId).Build());

        var result = await service.GetMatchesForUserAsync(firstUserId);

        const int expectedNumberOfMatches = 1;
        result.Should().HaveCount(expectedNumberOfMatches);
        result[0].User.UserId.Should().Be(firstUserId);
    }
}
