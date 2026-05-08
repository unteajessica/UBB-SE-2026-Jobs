using FluentAssertions;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Tests.ViewModels;

public class ShellViewModelTests
{
    [Fact]
    public void RecommendationsCommand_Executed_UpdatesActivePageAndStates()
    {
        var viewModel = new ShellViewModel(new SessionContext());

        viewModel.RecommendationsCommand.Execute(null);

        viewModel.ActivePage.Should().Be("Recommendations");
        viewModel.IsRecommendationsActive.Should().BeTrue();

        viewModel.ChatCommand.Execute(null);
        viewModel.IsChatActive.Should().BeTrue();
    }

    [Fact]
    public void CompanyModeCommand_Executed_UpdatesSessionModeToCompany()
    {
        var session = new SessionContext { Mode = AppMode.Candidate };
        var viewModel = new ShellViewModel(session);

        viewModel.CompanyModeCommand.Execute(null);

        session.Mode.Should().Be(AppMode.Company);
        viewModel.IsCompanyMode.Should().BeTrue();
        viewModel.IsCandidateMode.Should().BeFalse();
    }

    [Fact]
    public void DeveloperModeCommand_Executed_UpdatesSessionModeToDeveloper()
    {
        var session = new SessionContext { Mode = AppMode.Candidate };
        var viewModel = new ShellViewModel(session);

        viewModel.DeveloperModeCommand.Execute(null);

        session.Mode.Should().Be(AppMode.Developer);
        viewModel.IsDeveloperMode.Should().BeTrue();
        viewModel.IsCompanyMode.Should().BeFalse();
    }
}