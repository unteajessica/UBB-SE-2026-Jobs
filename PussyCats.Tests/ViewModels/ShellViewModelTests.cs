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

        Assert.Equal("Recommendations", viewModel.ActivePage);
        Assert.True(viewModel.IsRecommendationsActive);

        viewModel.ChatCommand.Execute(null);
        Assert.True(viewModel.IsChatActive);
    }

    [Fact]
    public void CompanyModeCommand_Executed_UpdatesSessionModeToCompany()
    {
        var session = new SessionContext { Mode = AppMode.Candidate };
        var viewModel = new ShellViewModel(session);

        viewModel.CompanyModeCommand.Execute(null);

        Assert.Equal(AppMode.Company, session.Mode);
        Assert.True(viewModel.IsCompanyMode);
        Assert.False(viewModel.IsCandidateMode);
    }

    [Fact]
    public void DeveloperModeCommand_Executed_UpdatesSessionModeToDeveloper()
    {
        var session = new SessionContext { Mode = AppMode.Candidate };
        var viewModel = new ShellViewModel(session);

        viewModel.DeveloperModeCommand.Execute(null);

        Assert.Equal(AppMode.Developer, session.Mode);
        Assert.True(viewModel.IsDeveloperMode);
        Assert.False(viewModel.IsCompanyMode);
    }
}