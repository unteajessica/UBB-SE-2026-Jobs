using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Library.Services.Users;
using PussyCats.Tests.Helpers;
using PussyCats.Web.Controllers;
using PussyCats.Web.Models;

namespace PussyCats.Tests.Web.Controllers;

public class RecommendationsControllerTests
{
    private readonly IRecommendationService recommendations = Substitute.For<IRecommendationService>();
    private readonly IUserService users = Substitute.For<IUserService>();
    private readonly IJobService jobs = Substitute.For<IJobService>();
    private readonly RecommendationsController controller;

    public RecommendationsControllerTests()
    {
        controller = new RecommendationsController(recommendations, users, jobs);
        users.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<User>());
        jobs.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Job>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithAllRecommendations()
    {
        var allRecommendations = new List<Recommendation>
        {
            new() { RecommendationId = 1, User = new UserBuilder().Build(), Job = new JobBuilder().Build() },
        };
        recommendations.GetAllAsync(Arg.Any<CancellationToken>()).Returns(allRecommendations);

        var result = await controller.Index(default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeEquivalentTo(allRecommendations);
    }

    [Fact]
    public async Task Details_RecommendationExists_ReturnsViewWithEntity()
    {
        var recommendation = new Recommendation { RecommendationId = 7, User = new UserBuilder().Build(), Job = new JobBuilder().Build() };
        recommendations.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(recommendation);

        var result = await controller.Details(7, default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(recommendation);
    }

    [Fact]
    public async Task Details_RecommendationMissing_ReturnsNotFound()
    {
        recommendations.GetByIdAsync(404, Arg.Any<CancellationToken>()).Returns((Recommendation?)null);

        var result = await controller.Details(404, default);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_Get_PopulatesDropdownsAndReturnsEmptyModel()
    {
        users.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { new UserBuilder().WithId(1).Build() });
        jobs.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { new JobBuilder().WithId(1).Build() });

        var result = await controller.Create(default);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<RecommendationFormModel>();
        (controller.ViewBag.Users as List<SelectListItem>).Should().HaveCount(1);
        (controller.ViewBag.Jobs as List<SelectListItem>).Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_Post_ValidModel_CallsServiceAndRedirectsToIndex()
    {
        var model = new RecommendationFormModel { UserId = 1, JobId = 2, Timestamp = DateTime.UtcNow };

        var result = await controller.Create(model, default);

        await recommendations.Received(1).AddAsync(1, 2, model.Timestamp, Arg.Any<CancellationToken>());
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(RecommendationsController.Index));
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
    {
        controller.ModelState.AddModelError("UserId", "required");
        var model = new RecommendationFormModel();

        var result = await controller.Create(model, default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
        await recommendations.DidNotReceiveWithAnyArgs().AddAsync(default, default, default, default);
    }

    [Fact]
    public async Task Create_Post_ServiceReturns404_AddsModelErrorAndReturnsView()
    {
        var model = new RecommendationFormModel { UserId = 1, JobId = 2, Timestamp = DateTime.UtcNow };
        recommendations.AddAsync(1, 2, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns<Task<Recommendation>>(_ => throw new HttpRequestException("not found", inner: null, statusCode: System.Net.HttpStatusCode.NotFound));

        var result = await controller.Create(model, default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
        controller.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Edit_Get_RecommendationExists_PopulatesFormModel()
    {
        var user = new UserBuilder().WithId(3).Build();
        var job = new JobBuilder().WithId(4).Build();
        var recommendation = new Recommendation
        {
            RecommendationId = 5,
            User = user,
            Job = job,
            Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        recommendations.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(recommendation);

        var result = await controller.Edit(5, default);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<RecommendationFormModel>().Subject;
        model.RecommendationId.Should().Be(5);
        model.UserId.Should().Be(3);
        model.JobId.Should().Be(4);
        model.Timestamp.Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Edit_Post_ValidModel_CallsServiceAndRedirects()
    {
        var model = new RecommendationFormModel { RecommendationId = 5, UserId = 1, JobId = 2, Timestamp = DateTime.UtcNow };

        var result = await controller.Edit(5, model, default);

        await recommendations.Received(1).UpdateTimestampAsync(5, model.Timestamp, Arg.Any<CancellationToken>());
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(RecommendationsController.Index));
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
    {
        var model = new RecommendationFormModel { RecommendationId = 99, Timestamp = DateTime.UtcNow };

        var result = await controller.Edit(5, model, default);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Delete_Get_RecommendationExists_ReturnsView()
    {
        var recommendation = new Recommendation { RecommendationId = 8, User = new UserBuilder().Build(), Job = new JobBuilder().Build() };
        recommendations.GetByIdAsync(8, Arg.Any<CancellationToken>()).Returns(recommendation);

        var result = await controller.Delete(8, default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(recommendation);
    }

    [Fact]
    public async Task DeleteConfirmed_CallsServiceAndRedirects()
    {
        var result = await controller.DeleteConfirmed(8, default);

        await recommendations.Received(1).RemoveAsync(8, Arg.Any<CancellationToken>());
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(RecommendationsController.Index));
    }
}
