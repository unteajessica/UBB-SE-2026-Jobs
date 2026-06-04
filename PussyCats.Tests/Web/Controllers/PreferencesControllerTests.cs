using System.Security.Claims;
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

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(prefs, view.Model);
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

        var model = Assert.IsType<PreferencesEditModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(prefs.Roles, model.SelectedRoles);
        Assert.Equal(WorkMode.Remote, model.WorkMode);
        Assert.Equal("Berlin, Germany", model.Location);
    }

    [Fact]
    public async Task EditPost_InvalidModel_ReturnsViewWithoutSaving()
    {
        var model = new PreferencesEditModel { SelectedRoles = new List<JobRole>() };
        controller.ModelState.AddModelError("SelectedRoles", "required");

        var result = await controller.Edit(model, default);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
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
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PreferencesController.Index), redirect.ActionName);
    }
}
