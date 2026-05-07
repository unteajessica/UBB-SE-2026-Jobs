using FluentAssertions;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Tests.ViewModels;

public class ShellViewModelTests
{
    [Fact]
    public void Commands_update_active_page()
    {
        var viewModel = new ShellViewModel(new SessionContext());

        viewModel.RecommendationsCommand.Execute(null);

        viewModel.ActivePage.Should().Be("Recommendations");
        viewModel.IsRecommendationsActive.Should().BeTrue();

        viewModel.ChatCommand.Execute(null);
        viewModel.IsChatActive.Should().BeTrue();
    }

    [Fact]
    public void Mode_commands_update_session_mode()
    {
        var session = new SessionContext { Mode = AppMode.Candidate };
        var viewModel = new ShellViewModel(session);

        viewModel.CompanyModeCommand.Execute(null);

        session.Mode.Should().Be(AppMode.Company);
        viewModel.IsCompanyMode.Should().BeTrue();
        viewModel.IsCandidateMode.Should().BeFalse();
    }
}
