using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.PersonalityTestService;

namespace PussyCats.Tests.Integration;

public class PersonalityTestViewModelTests
{
    private readonly IPersonalityTestService service = Substitute.For<IPersonalityTestService>();
    private readonly SessionContext session = new() { UserId = 11 };

    [Fact]
    public void CanSubmit_AllQuestionsAnswered_ReturnsTrue()
    {
        var viewModel = new PersonalityTestViewModel(session, service);

        viewModel.CanSubmit.Should().BeFalse();

        foreach (var question in viewModel.Questions)
        {
            question.SelectedAnswer = (int)AnswerValue.Agree;
        }

        viewModel.CanSubmit.Should().BeTrue();
    }

    [Fact]
    public void SubmitCommand_ValidAnswers_CalculatesAndExposesTopRoles()
    {
        service.CalculateTraitScores(Arg.Any<IReadOnlyDictionary<Question, AnswerValue>>())
            .Returns(new Dictionary<TraitType, double> { [TraitType.Abstraction] = 5 });
        service.CalculateRoleScores(Arg.Any<IReadOnlyDictionary<TraitType, double>>())
            .Returns(new Dictionary<JobRole, double> { [JobRole.BackendDeveloper] = 91, [JobRole.FrontendDeveloper] = 77 });
        service.GetTopRoles(Arg.Any<IReadOnlyDictionary<JobRole, double>>(), 3)
            .Returns(new Dictionary<JobRole, double> { [JobRole.BackendDeveloper] = 91, [JobRole.FrontendDeveloper] = 77 });

        var viewModel = new PersonalityTestViewModel(session, service);
        foreach (var question in viewModel.Questions)
        {
            question.SelectedAnswer = (int)AnswerValue.Agree;
        }

        viewModel.SubmitCommand.Execute(null);

        viewModel.IsTestSubmitted.Should().BeTrue();
        viewModel.TopRoles.Should().HaveCount(2);
        viewModel.TopRoles[0].Role.Should().Be(JobRole.BackendDeveloper);
    }

    [Fact]
    public async Task SaveResultCommand_RoleSelected_PersistsSelectedRoleViaService()
    {
        service.CalculateTraitScores(Arg.Any<IReadOnlyDictionary<Question, AnswerValue>>())
            .Returns(new Dictionary<TraitType, double> { [TraitType.Abstraction] = 5 });
        service.CalculateRoleScores(Arg.Any<IReadOnlyDictionary<TraitType, double>>())
            .Returns(new Dictionary<JobRole, double> { [JobRole.BackendDeveloper] = 91 });
        service.GetTopRoles(Arg.Any<IReadOnlyDictionary<JobRole, double>>(), 3)
            .Returns(new Dictionary<JobRole, double> { [JobRole.BackendDeveloper] = 91 });

        var viewModel = new PersonalityTestViewModel(session, service);
        foreach (var question in viewModel.Questions)
        {
            question.SelectedAnswer = (int)AnswerValue.StronglyAgree;
        }
        viewModel.SubmitCommand.Execute(null);

        viewModel.SelectRoleCommand.Execute(viewModel.TopRoles[0]);
        await viewModel.SaveResultCommand.ExecuteAsync(null);

        await service.Received(1).SaveResultAsync(
            11,
            Arg.Is<IReadOnlyDictionary<Question, AnswerValue>>(answers => answers.Count == viewModel.Questions.Count),
            JobRole.BackendDeveloper,
            Arg.Any<CancellationToken>());

        viewModel.SaveMessage.Should().Contain("Backend Developer");
        viewModel.SelectedRole.Should().Be(viewModel.TopRoles[0]);
        viewModel.TopRoles[0].IsSelected.Should().BeTrue();
    }
}