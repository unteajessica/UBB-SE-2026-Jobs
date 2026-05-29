using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.Preferences;

namespace PussyCats.Tests.ViewModels;

public class PreferencesViewModelTests
{
    private readonly IPreferenceService preferenceService = Substitute.For<IPreferenceService>();
    private readonly SessionContext session = new() { UserId = 3 };

    [Fact]
    public async Task LoadPreferencesAsync_PreferencesExist_PopulatesRolesWorkModeAndLocation()
    {
        preferenceService.GetByUserIdAsync(3, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new UserPreferences(
                [JobRole.BackendDeveloper],
                WorkMode.Remote,
                "Cluj-Napoca")));
        var viewModel = new PreferencesViewModel(preferenceService, session);

        await viewModel.LoadPreferencesAsync();

        viewModel.GetSelectedJobRoles().Should().ContainSingle().Which.Should().Be(JobRole.BackendDeveloper);
        viewModel.GetSelectedWorkMode().Should().Be(WorkMode.Remote);
        viewModel.GetPreferredLocation().Should().Be("Cluj-Napoca");
    }

    [Fact]
    public void ToggleJobRole_ExceedingLimit_EnforcesThreeRoleLimitAndShowsError()
    {
        var viewModel = new PreferencesViewModel(preferenceService, session);

        viewModel.ToggleJobRole(JobRole.BackendDeveloper);
        viewModel.ToggleJobRole(JobRole.FrontendDeveloper);
        viewModel.ToggleJobRole(JobRole.DevOpsEngineer);
        viewModel.ToggleJobRole(JobRole.DataAnalyst);

        viewModel.GetSelectedJobRoles().Should().HaveCount(3);
        viewModel.GetErrorMessage().Should().Contain("maximum of 3");
    }

    [Fact]
    public async Task SearchLocationAsync_ValidQuery_PopulatesLocationSuggestions()
    {
        preferenceService.SearchLocationsAsync("cl", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<string>>(["Cluj-Napoca"]));
        var viewModel = new PreferencesViewModel(preferenceService, session);

        await viewModel.SearchLocationAsync("cl");

        viewModel.GetLocationSuggestions().Should().ContainSingle("Cluj-Napoca");
    }

    [Fact]
    public async Task SavePreferencesAsync_CommandExecuted_PassesSelectionsToService()
    {
        var viewModel = new PreferencesViewModel(preferenceService, session);
        viewModel.ToggleJobRole(JobRole.BackendDeveloper);
        viewModel.SetWorkMode(WorkMode.Hybrid);
        viewModel.SetLocation("Bucharest");

        await viewModel.SavePreferencesAsync();

        await preferenceService.Received(1).SavePreferencesAsync(
            3,
            Arg.Is<IReadOnlyList<JobRole>>(roles => roles.SequenceEqual(new[] { JobRole.BackendDeveloper })),
            WorkMode.Hybrid,
            "Bucharest",
            Arg.Any<CancellationToken>());
    }
}