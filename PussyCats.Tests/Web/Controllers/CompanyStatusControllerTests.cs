using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.Matches;
using PussyCats.Tests.Helpers;
using PussyCats.Web.Configuration;
using PussyCats.Web.Controllers;
using PussyCats.Web.Models;

namespace PussyCats.Tests.Web.Controllers;

public class CompanyStatusControllerTests
{
    private readonly ICompanyStatusService companyStatusService = Substitute.For<ICompanyStatusService>();
    private readonly IMatchService matchService = Substitute.For<IMatchService>();
    private readonly CompanyStatusController controller;

    public CompanyStatusControllerTests()
    {
        controller = new CompanyStatusController(
            companyStatusService,
            matchService,
            new ApiConfiguration("http://localhost") { TemporaryCompanyId = 12 });
    }

    [Fact]
    public async Task Index_UsesTemporaryCompanyId_ReturnsApplicants()
    {
        var applicants = new List<UserApplicationResult> { BuildApplicant(7) };
        companyStatusService.GetApplicantsForCompanyAsync(12, Arg.Any<CancellationToken>())
            .Returns(applicants);

        var result = await controller.Index(default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeEquivalentTo(applicants);
    }

    [Fact]
    public async Task Details_MissingApplicant_ReturnsNotFound()
    {
        companyStatusService.GetApplicantByMatchIdAsync(12, 404, Arg.Any<CancellationToken>())
            .Returns((UserApplicationResult?)null);

        var result = await controller.Details(404, default);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Get_ReturnsDecisionForm()
    {
        var applicant = BuildApplicant(9, MatchStatus.Advanced);
        companyStatusService.GetApplicantByMatchIdAsync(12, 9, Arg.Any<CancellationToken>())
            .Returns(applicant);

        var result = await controller.Edit(9, default);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<MatchDecisionFormModel>().Subject;
        model.MatchId.Should().Be(9);
        model.ApplicantName.Should().Be("Ada Lovelace");
        model.JobTitle.Should().Be("Backend Engineer");
        model.CompanyName.Should().Be("PussyCats");
    }

    [Fact]
    public async Task Edit_Post_InvalidDecision_ReturnsViewAndDoesNotCallService()
    {
        var model = new MatchDecisionFormModel
        {
            MatchId = 5,
            Decision = MatchStatus.Applied,
            Feedback = "Needs review",
        };

        var result = await controller.Edit(5, model, default);

        await matchService.DidNotReceiveWithAnyArgs().SubmitDecisionAsync(default, default, default!, default);
        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
    }

    [Fact]
    public async Task Edit_Post_ValidDecision_SubmitsDecisionAndRedirects()
    {
        var model = new MatchDecisionFormModel
        {
            MatchId = 5,
            Decision = MatchStatus.Accepted,
            Feedback = "  Welcome aboard  ",
        };

        var result = await controller.Edit(5, model, default);

        await matchService.Received(1).SubmitDecisionAsync(
            5,
            MatchStatus.Accepted,
            "Welcome aboard",
            Arg.Any<CancellationToken>());
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(CompanyStatusController.Details));
        redirect.RouteValues!["id"].Should().Be(5);
    }

    private static UserApplicationResult BuildApplicant(int matchId, MatchStatus status = MatchStatus.Applied)
    {
        var user = new UserBuilder().WithId(11).WithName("Ada", "Lovelace").Build();
        var job = new JobBuilder().WithId(21).WithCompanyId(12).WithTitle("Backend Engineer").Build();
        job.Company.CompanyName = "PussyCats";

        return new UserApplicationResult
        {
            User = user,
            Match = new MatchBuilder().WithId(matchId).WithStatus(status).Build(),
            Job = job,
            CompatibilityScore = 88,
            Feedback = status is MatchStatus.Accepted or MatchStatus.Rejected ? "Feedback" : string.Empty,
        };
    }
}
