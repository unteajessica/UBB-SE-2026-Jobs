using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Matches;
using PussyCats.Tests.Helpers;
using PussyCats.Web.Configuration;
using PussyCats.Web.Controllers;
using PussyCats.Web.Models;

namespace PussyCats.Tests.Web.Controllers;

public class MatchesControllerTests
{
    private readonly IMatchService matches = Substitute.For<IMatchService>();
    private readonly MatchesController controller;

    public MatchesControllerTests()
    {
        controller = new MatchesController(
            matches,
            new ApiConfiguration("http://localhost") { TemporaryCompanyId = 42 });
    }

    [Fact]
    public async Task Index_UsesTemporaryCompanyId_ReturnsCompanyMatches()
    {
        var companyMatches = new List<Match> { CreateMatch(matchId: 7) };
        matches.GetByCompanyIdAsync(42, Arg.Any<CancellationToken>()).Returns(companyMatches);

        var result = await controller.Index(default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeEquivalentTo(companyMatches);
    }

    [Fact]
    public async Task Details_MatchMissing_ReturnsNotFound()
    {
        matches.GetByIdAsync(404, Arg.Any<CancellationToken>()).Returns((Match?)null);

        var result = await controller.Details(404, default);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Decision_Get_MatchExists_ReturnsDecisionForm()
    {
        var match = CreateMatch(matchId: 3, status: MatchStatus.Advanced);
        matches.GetByIdAsync(3, Arg.Any<CancellationToken>()).Returns(match);

        var result = await controller.Decision(3, default);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<MatchDecisionFormModel>().Subject;
        model.MatchId.Should().Be(3);
        model.CurrentStatus.Should().Be(MatchStatus.Advanced);
        model.ApplicantName.Should().Be("Ada Lovelace");
        model.JobTitle.Should().Be("Backend Engineer");
    }

    [Fact]
    public async Task Decision_Post_ValidModel_CallsServiceWithTrimmedFeedbackAndRedirectsToDetails()
    {
        var model = new MatchDecisionFormModel
        {
            MatchId = 5,
            Decision = MatchStatus.Accepted,
            Feedback = "  Welcome aboard  ",
        };

        var result = await controller.Decision(5, model, default);

        await matches.Received(1).SubmitDecisionAsync(
            5,
            MatchStatus.Accepted,
            "Welcome aboard",
            Arg.Any<CancellationToken>());
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(MatchesController.Details));
        redirect.RouteValues!["id"].Should().Be(5);
    }

    [Fact]
    public async Task Decision_Post_InvalidModel_DoesNotCallServiceAndReturnsView()
    {
        var match = CreateMatch(matchId: 5);
        matches.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(match);
        controller.ModelState.AddModelError(nameof(MatchDecisionFormModel.Feedback), "required");
        var model = new MatchDecisionFormModel
        {
            MatchId = 5,
            Decision = MatchStatus.Accepted,
            Feedback = string.Empty,
        };

        var result = await controller.Decision(5, model, default);

        await matches.DidNotReceiveWithAnyArgs().SubmitDecisionAsync(default, default, default!, default);
        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
    }

    [Fact]
    public async Task Decision_Post_ServiceReturnsNotFound_ReturnsNotFound()
    {
        var model = new MatchDecisionFormModel
        {
            MatchId = 5,
            Decision = MatchStatus.Accepted,
            Feedback = "Accepted",
        };
        matches.SubmitDecisionAsync(5, MatchStatus.Accepted, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new HttpRequestException("missing", null, HttpStatusCode.NotFound)));

        var result = await controller.Decision(5, model, default);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Decision_Post_ServiceRejectsTransition_AddsModelErrorAndReturnsView()
    {
        var match = CreateMatch(matchId: 5, status: MatchStatus.Accepted);
        matches.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(match);
        var model = new MatchDecisionFormModel
        {
            MatchId = 5,
            Decision = MatchStatus.Rejected,
            Feedback = "No longer eligible",
        };
        matches.SubmitDecisionAsync(5, MatchStatus.Rejected, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new HttpRequestException("invalid transition", null, HttpStatusCode.UnprocessableEntity)));

        var result = await controller.Decision(5, model, default);

        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
        controller.ModelState.IsValid.Should().BeFalse();
    }

    private static Match CreateMatch(int matchId = 1, MatchStatus status = MatchStatus.Applied)
    {
        var user = new UserBuilder()
            .WithId(11)
            .WithName("Ada", "Lovelace")
            .WithEmail("ada@example.com")
            .Build();

        var job = new JobBuilder()
            .WithId(21)
            .WithCompanyId(42)
            .WithTitle("Backend Engineer")
            .Build();
        job.Company.CompanyName = "PussyCats";

        return new Match
        {
            MatchId = matchId,
            User = user,
            Job = job,
            Status = status,
            Timestamp = new DateTime(2026, 5, 21, 12, 0, 0, DateTimeKind.Utc),
            FeedbackMessage = status is MatchStatus.Accepted or MatchStatus.Rejected ? "Previous feedback" : string.Empty,
        };
    }
}
