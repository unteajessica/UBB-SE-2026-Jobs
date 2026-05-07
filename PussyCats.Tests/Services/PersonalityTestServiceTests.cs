using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class PersonalityTestServiceTests
{
    private readonly FakePersonalityTestRepository repo = new();
    private readonly PersonalityTestService service;

    public PersonalityTestServiceTests()
    {
        service = new PersonalityTestService(repo);
    }

    [Fact]
    public void LoadQuestions_Called_Returns24QuestionsInSortOrder()
    {
        var questions = PersonalityTestService.LoadQuestions();

        questions.Should().HaveCount(24);
        questions.Select(q => q.SortOrder).Should().BeInAscendingOrder();
        questions.Select(q => q.SortOrder).Should().Equal(Enumerable.Range(1, 24));
    }

    [Fact]
    public void LoadQuestions_Called_CoversAllSixTraitsEvenly()
    {
        var questions = PersonalityTestService.LoadQuestions();
        var perTrait = questions.GroupBy(q => q.Trait).ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

        perTrait.Should().HaveCount(6);
        perTrait.Values.Should().AllBeEquivalentTo(4);
    }

    [Fact]
    public void CalculateTraitScores_AnswersProvided_AveragesScoresPerTrait()
    {
        var q1 = new Question { QuestionText = "x", Trait = TraitType.Visibility, SortOrder = 1 };
        var q2 = new Question { QuestionText = "y", Trait = TraitType.Visibility, SortOrder = 2 };
        var q3 = new Question { QuestionText = "z", Trait = TraitType.Depth, SortOrder = 3 };

        var answers = new Dictionary<Question, AnswerValue>
        {
            [q1] = AnswerValue.StronglyAgree,
            [q2] = AnswerValue.Agree,
            [q3] = AnswerValue.Neutral,
        };

        var scores = service.CalculateTraitScores(answers);

        scores[TraitType.Visibility].Should().Be(4.5);
        scores[TraitType.Depth].Should().Be(3);
    }

    [Fact]
    public void CalculateRoleScores_TraitScoresProvided_ReturnsScoreForEveryRole()
    {
        var traitScores = new Dictionary<TraitType, double>
        {
            [TraitType.Visibility] = 3,
            [TraitType.Interaction] = 3,
            [TraitType.Depth] = 3,
            [TraitType.Creativity] = 3,
            [TraitType.Pace] = 3,
            [TraitType.Abstraction] = 3,
        };

        var roleScores = service.CalculateRoleScores(traitScores);

        roleScores.Should().HaveCount(8);
        roleScores.Keys.Should().BeEquivalentTo(Enum.GetValues<JobRole>());
    }

    [Fact]
    public void GetTopRoles_RoleScoresProvided_ReturnsHighestScoringRolesInDescendingOrder()
    {
        var scores = new Dictionary<JobRole, double>
        {
            [JobRole.BackendDeveloper] = 5,
            [JobRole.FrontendDeveloper] = 10,
            [JobRole.DataAnalyst] = 8,
        };

        var top2 = service.GetTopRoles(scores, 2);

        top2.Should().HaveCount(2);
        top2.Keys.First().Should().Be(JobRole.FrontendDeveloper);
        top2.Keys.Last().Should().Be(JobRole.DataAnalyst);
    }

    [Fact]
    public async Task SaveResultAsync_ValidAnswersAndRole_PersistsWithSelectedRoleAndRoundedTraitScores()
    {
        var q1 = new Question { Trait = TraitType.Depth, SortOrder = 1 };
        var q2 = new Question { Trait = TraitType.Depth, SortOrder = 2 };
        var q3 = new Question { Trait = TraitType.Depth, SortOrder = 3 };
        var answers = new Dictionary<Question, AnswerValue>
        {
            [q1] = AnswerValue.StronglyAgree,
            [q2] = AnswerValue.StronglyAgree,
            [q3] = AnswerValue.StronglyAgree,
        };

        await service.SaveResultAsync(1, answers, JobRole.BackendDeveloper);

        var saved = await repo.GetByUserIdAsync(1);
        saved.Should().NotBeNull();
        saved!.SelectedRole.Should().Be(JobRole.BackendDeveloper);
        saved.UserId.Should().Be(1);
        saved.TraitScores.Should().Contain(s => s.Trait == TraitType.Depth && s.Score == 5);
    }

    [Fact]
    public async Task SaveResultAsync_ResultAlreadyExistsForUser_UpdatesExistingResultKeepingId()
    {
        repo.Seed(new PersonalityTestResult
        {
            PersonalityTestResultId = 7,
            UserId = 1,
            SelectedRole = JobRole.FrontendDeveloper,
        });
        var q = new Question { Trait = TraitType.Depth, SortOrder = 1 };
        var answers = new Dictionary<Question, AnswerValue> { [q] = AnswerValue.StronglyAgree };

        await service.SaveResultAsync(1, answers, JobRole.BackendDeveloper);

        var saved = await repo.GetByUserIdAsync(1);
        saved!.PersonalityTestResultId.Should().Be(7);
        saved.SelectedRole.Should().Be(JobRole.BackendDeveloper);
    }
}