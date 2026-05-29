using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Preferences;
using PussyCats.Web.Controllers;
using PussyCats.Web.Models;

namespace PussyCats.Tests.Web.Controllers;

public class PreferencesControllerTests
{
    private readonly IPreferenceService preferences = Substitute.For<IPreferenceService>();
    private readonly PreferencesController controller;

    public PreferencesControllerTests()
    {
        controller = new PreferencesController(preferences);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "test"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user },
        };
    }

    [Fact]
    public async Task Index_ReturnsViewWithUserPreferences()
    {
        var prefs = new UserPreferences(
            new List<JobRole> { JobRole.BackendDeveloper },
            WorkMode.Hybrid,
            "Cluj-Napoca, Romania");
        preferences.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(prefs);

        var result = await controller.Index(default);

        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeSameAs(prefs);
    }

    [Fact]
    public async Task EditGet_LoadsCurrentPreferencesIntoEditModel()
    {
        var prefs = new UserPreferences(
            new List<JobRole> { JobRole.FrontendDeveloper, JobRole.DataAnalyst },
            WorkMode.Remote,
            "Berlin, Germany");
        preferences.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(prefs);

        var result = await controller.Edit(default);

        var model = result.Should().BeOfType<ViewResult>().Which.Model
            .Should().BeOfType<PreferencesEditModel>().Subject;
        model.SelectedRoles.Should().BeEquivalentTo(prefs.Roles);
        model.WorkMode.Should().Be(WorkMode.Remote);
        model.Location.Should().Be("Berlin, Germany");
    }

    [Fact]
    public async Task EditPost_InvalidModel_ReturnsViewWithoutSaving()
    {
        var model = new PreferencesEditModel { SelectedRoles = new List<JobRole>() };
        controller.ModelState.AddModelError("SelectedRoles", "required");

        var result = await controller.Edit(model, default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(model);
        await preferences.DidNotReceive().SavePreferencesAsync(
            Arg.Any<int>(),
            Arg.Any<IReadOnlyList<JobRole>>(),
            Arg.Any<WorkMode>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditPost_ValidModel_SavesPreferencesAndRedirectsToIndex()
    {
        var model = new PreferencesEditModel
        {
            SelectedRoles = new List<JobRole> { JobRole.AiMlEngineer },
            WorkMode = WorkMode.OnSite,
            Location = "Bucharest, Romania",
        };

        var result = await controller.Edit(model, default);

        await preferences.Received(1).SavePreferencesAsync(
            1,
            Arg.Is<IReadOnlyList<JobRole>>(roles => roles.Single() == JobRole.AiMlEngineer),
            WorkMode.OnSite,
            "Bucharest, Romania",
            Arg.Any<CancellationToken>());
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be(nameof(PreferencesController.Index));
    }
}
