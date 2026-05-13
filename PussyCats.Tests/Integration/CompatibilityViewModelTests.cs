using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats_App.Services.CompatibilityService;

namespace PussyCats.Tests.Integration;

public class CompatibilityViewModelTests
{
    private readonly FakeUserRepository userRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
    private readonly FakeJobSkillRepository jobSkillRepo = new();
    private readonly FakeJobRepository jobRepo = new();

    [Fact]
    public async Task LoadAllRolesAsync_RolesExist_PopulatesResultsAndTracksSelection()
    {
        var companyId = 4;
        var applicantResult = ViewModelTestData.Applicant(matchId: 7, companyId: companyId, status: MatchStatus.Applied);

        userRepo.Seed(applicantResult.User);
        jobRepo.Seed(applicantResult.Job);

        var service = Substitute.For<ICompatibilityService>();
        var result = new RoleResult
        {
            JobRole = JobRole.BackendDeveloper,
            MatchScore = 88,
        };
        service.CalculateAllAsync(applicantResult.User.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RoleResult>>([result]));

        var viewModel = new CompatibilityOverviewViewModel(service, new SessionContext { UserId = applicantResult.User.UserId });

        await viewModel.LoadAllRolesAsync();
        viewModel.OnRoleSelected(JobRole.BackendDeveloper);

        viewModel.GetRoleResults().Should().ContainSingle().Which.Should().BeSameAs(result);
        viewModel.GetSelectedResult().Should().BeSameAs(result);
        viewModel.GetErrorMessage().Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAllRolesAsync_ServiceThrowsException_CapturesErrorMessage()
    {
        var userId = 14;
        var service = Substitute.For<ICompatibilityService>();
        service.CalculateAllAsync(userId, Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<RoleResult>>>(_ => throw new InvalidOperationException("no profile"));

        var viewModel = new CompatibilityOverviewViewModel(service, new SessionContext { UserId = userId });

        await viewModel.LoadAllRolesAsync();

        viewModel.GetRoleResults().Should().BeEmpty();
        viewModel.GetErrorMessage().Should().Be("no profile");
    }

    [Fact]
    public void LoadResult_ValidData_FormatsRoleNameAndSuggestions()
    {
        var result = new RoleResult
        {
            JobRole = JobRole.UiUxDesigner,
            MatchScore = 72,
            Suggestions = [new Suggestion { SkillName = "Portfolio", GroupName = "Design", GainScore = 12 }],
        };
        var viewModel = new CompatibilityDetailViewModel();

        viewModel.LoadResult(result);

        viewModel.GetMatchScore().Should().Be(72);
        viewModel.GetRoleName().Should().Be("UI/UX Designer");
        viewModel.GetSuggestions().Should().ContainSingle();
    }
}