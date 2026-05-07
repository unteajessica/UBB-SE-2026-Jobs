using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.Tests.ViewModels;

public class CompatibilityViewModelTests
{
    [Fact]
    public async Task Overview_loads_roles_and_tracks_selection()
    {
        var service = Substitute.For<ICompatibilityService>();
        var result = new RoleResult
        {
            JobRole = JobRole.BackendDeveloper,
            MatchScore = 88,
        };
        service.CalculateAllAsync(14, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RoleResult>>([result]));
        var viewModel = new CompatibilityOverviewViewModel(service, new SessionContext { UserId = 14 });

        await viewModel.LoadAllRolesAsync();
        viewModel.OnRoleSelected(JobRole.BackendDeveloper);

        viewModel.GetRoleResults().Should().ContainSingle().Which.Should().BeSameAs(result);
        viewModel.GetSelectedResult().Should().BeSameAs(result);
        viewModel.GetErrorMessage().Should().BeEmpty();
    }

    [Fact]
    public async Task Overview_captures_service_error_message()
    {
        var service = Substitute.For<ICompatibilityService>();
        service.CalculateAllAsync(14, Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<RoleResult>>>(_ => throw new InvalidOperationException("no profile"));
        var viewModel = new CompatibilityOverviewViewModel(service, new SessionContext { UserId = 14 });

        await viewModel.LoadAllRolesAsync();

        viewModel.GetRoleResults().Should().BeEmpty();
        viewModel.GetErrorMessage().Should().Be("no profile");
    }

    [Fact]
    public void Detail_loads_role_result_and_formats_role_name()
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
