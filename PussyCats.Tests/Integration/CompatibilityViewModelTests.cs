using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.CompatibilityService;

namespace PussyCats.Tests.Integration;

public class CompatibilityViewModelTests
{
    private readonly FakeUserRepository userRepository = new();
    private readonly FakeUserSkillRepository userSkillRepository = new();
    private readonly FakeJobSkillRepository jobSkillRepository = new();
    private readonly FakeJobRepository jobRepository = new();

    [Fact]
    public async Task LoadAllRolesAsync_RolesExist_PopulatesResultsAndTracksSelection()
    {
        var companyId = 4;
        var applicantResult = ViewModelTestData.Applicant(matchId: 7, companyId: companyId, status: MatchStatus.Applied);

        userRepository.Seed(applicantResult.User);
        jobRepository.Seed(applicantResult.Job);

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

        Assert.Same(result, Assert.Single(viewModel.GetRoleResults()));
        Assert.Same(result, viewModel.GetSelectedResult());
        Assert.Empty(viewModel.GetErrorMessage());
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

        Assert.Empty(viewModel.GetRoleResults());
        Assert.Equal("no profile", viewModel.GetErrorMessage());
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

        Assert.Equal(72, viewModel.GetMatchScore());
        Assert.Equal("UI/UX Designer", viewModel.GetRoleName());
        Assert.Single(viewModel.GetSuggestions());
    }
}