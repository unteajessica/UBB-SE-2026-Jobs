using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.CompanyRecommendationService;
using PussyCats.Tests.Helpers;
using PussyCats.Web.Configuration;
using PussyCats.Web.Controllers;
using PussyCats.Web.Models;

namespace PussyCats.Tests.Web.Controllers;

public class CompanyRecommendationsControllerTests
{
    private readonly ICompanyRecommendationService companyRecommendations = Substitute.For<ICompanyRecommendationService>();
    private readonly CompanyRecommendationsController controller;

    public CompanyRecommendationsControllerTests()
    {
        controller = new CompanyRecommendationsController(
            companyRecommendations,
            new ApiConfiguration("http://localhost") { TemporaryCompanyId = 42 });
    }

    [Fact]
    public async Task Index_UsesTemporaryCompanyId_ReturnsRankedApplicants()
    {
        var applicants = new List<UserApplicationResult> { BuildApplicant(7) };
        companyRecommendations.GetRankedApplicantsAsync(42, Arg.Any<CancellationToken>()).Returns(applicants);

        var result = await controller.Index(default);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(applicants, view.Model);
    }

    [Fact]
    public async Task Details_MissingApplicant_ReturnsNotFound()
    {
        companyRecommendations.GetApplicantByMatchIdAsync(42, 404, Arg.Any<CancellationToken>())
            .Returns((UserApplicationResult?)null);

        var result = await controller.Details(404, default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ExistingApplicant_ReturnsApplicantWithBreakdown()
    {
        var applicant = BuildApplicant(7);
        var breakdown = new CompatibilityBreakdown { OverallScore = 91 };
        companyRecommendations.GetApplicantByMatchIdAsync(42, 7, Arg.Any<CancellationToken>())
            .Returns(applicant);
        companyRecommendations.GetBreakdownAsync(applicant, Arg.Any<CancellationToken>())
            .Returns(breakdown);

        var result = await controller.Details(7, default);

        var model = Assert.IsType<CompanyRecommendationDetailsModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(applicant, model.Applicant);
        Assert.Equal(breakdown, model.Breakdown);
    }

    private static UserApplicationResult BuildApplicant(int matchId)
    {
        var user = new UserBuilder().WithId(11).WithName("Ada", "Lovelace").Build();
        var job = new JobBuilder().WithId(21).WithCompanyId(42).WithTitle("Backend Engineer").Build();

        return new UserApplicationResult
        {
            User = user,
            Match = new Match
            {
                MatchId = matchId,
                User = user,
                Job = job,
                Timestamp = DateTime.UtcNow,
            },
            Job = job,
            CompatibilityScore = 88,
            UserSkills = new List<UserSkill>(),
        };
    }
}
