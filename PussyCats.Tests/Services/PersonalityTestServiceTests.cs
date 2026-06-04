using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.PersonalityTestService;

namespace PussyCats.Tests.Services;

public class PersonalityTestServiceTests
{
    private readonly FakePersonalityTestRepository personalityTestRepository = new();
    private readonly PersonalityTestService service;

    public PersonalityTestServiceTests()
    {
        service = new PersonalityTestService(personalityTestRepository);
    }

    [Fact]
    public void LoadQuestions_Called_ReturnsQuestionsInSortOrder()
    {

        var questions = PersonalityTestService.LoadQuestions();

        var sortOrders = questions.Select(question => question.SortOrder).ToList();
        Assert.Equal(sortOrders.OrderBy(x => x).ToList(), sortOrders);
    }

    [Fact]
    public void LoadQuestions_Called_ReturnsCountSixTrait()
        
    {
        const int expectedTraitCount = 6;

        var questions = PersonalityTestService.LoadQuestions();
        var perTrait = questions.GroupBy(question => question.Trait).ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

        Assert.Equal(expectedTraitCount, perTrait.Count());
    }

    [Fact]
    public void CalculateTraitScores_AnswersProvided_AveragesScoresPerTrait()
    {
        const double expectedVisibilityAverage = 4.5; //StronglyAgree=5, Agree=4 => (5+4)/2
        var visibleQuestion1 = new Question { QuestionText = "x", Trait = TraitType.Visibility, SortOrder = 1 };
        var visibleQuestion2 = new Question { QuestionText = "y", Trait = TraitType.Visibility, SortOrder = 2 };
        var visibleQuestion3 = new Question { QuestionText = "z", Trait = TraitType.Depth, SortOrder = 3 };

        var answers = new Dictionary<Question, AnswerValue>
        {
            [visibleQuestion1] = AnswerValue.StronglyAgree,
            [visibleQuestion2] = AnswerValue.Agree,
            [visibleQuestion3] = AnswerValue.Neutral,
        };

        var scores = service.CalculateTraitScores(answers);

        Assert.Equal(expectedVisibilityAverage, scores[TraitType.Visibility]);
    }

    [Fact]
    public void CalculateRoleScores_TraitScoresProvided_ReturnsScoreForEveryRole()
    {
        const double neutralTraitScore = 3;
        const int expectedRoleCount= 8;

        var traitScores = new Dictionary<TraitType, double>
        {
            [TraitType.Visibility] = neutralTraitScore,
            [TraitType.Interaction] = neutralTraitScore,
            [TraitType.Depth] = neutralTraitScore,
            [TraitType.Creativity] = neutralTraitScore,
            [TraitType.Pace] = neutralTraitScore,
            [TraitType.Abstraction] = neutralTraitScore,
        };

        var roleScores = service.CalculateRoleScores(traitScores);

        Assert.Equal(expectedRoleCount, roleScores.Count());
    }

    [Fact]
    public void CalculateRoleScores_TraitScoresProvided_ContainsAllDefinedJobRoles()
    {
        const double neutralTraitScore = 3;

        var traitScores = new Dictionary<TraitType, double>
        {
            [TraitType.Visibility] = neutralTraitScore,
            [TraitType.Interaction] = neutralTraitScore,
            [TraitType.Depth] = neutralTraitScore,
            [TraitType.Creativity] = neutralTraitScore,
            [TraitType.Pace] = neutralTraitScore,
            [TraitType.Abstraction] = neutralTraitScore,
        };

        var roleScores = service.CalculateRoleScores(traitScores);

        Assert.Equal(Enum.GetValues<JobRole>().OrderBy(role => role).ToList(), roleScores.Keys.OrderBy(role => role).ToList());
    }

    [Fact]
    public void GetTopRoles_RoleScoresProvided_ReturnsRequestedCount()
    {
        const int requestedTopCount = 2;

        var scores = new Dictionary<JobRole, double>
        {
            [JobRole.BackendDeveloper] = 5,
            [JobRole.FrontendDeveloper] = 10,
            [JobRole.DataAnalyst] = 8,
        };

        var topRoles = service.GetTopRoles(scores, requestedTopCount);

        Assert.Equal(requestedTopCount, topRoles.Count());
    }

    [Fact]
    public void GetTopRoles_RoleScoresProvided_ReturnsHighestScoringRoleFirst()
    {
        const int requestedTopCount = 2;

        var scores = new Dictionary<JobRole, double>
        {
            [JobRole.BackendDeveloper] = 5,
            [JobRole.FrontendDeveloper] = 10,
            [JobRole.DataAnalyst] = 8,
        };

        var topRoles = service.GetTopRoles(scores, requestedTopCount);

        Assert.Equal(JobRole.FrontendDeveloper, topRoles.Keys.First());
    }

    [Fact]
    public async Task SaveResultAsync_ValidAnswersAndRole_PersistsWithSelectedRole()
    {
        const int userId = 1;
        const JobRole selectedRole = JobRole.BackendDeveloper;

        var depthQuestion1 = new Question { Trait = TraitType.Depth, SortOrder = 1 };
        var depthQuestion2 = new Question { Trait = TraitType.Depth, SortOrder = 2 };
        var depthQuestion3 = new Question { Trait = TraitType.Depth, SortOrder = 3 };
        var answers = new Dictionary<Question, AnswerValue>
        {
            [depthQuestion1] = AnswerValue.StronglyAgree,
            [depthQuestion2] = AnswerValue.StronglyAgree,
            [depthQuestion3] = AnswerValue.StronglyAgree,
        };

        await service.SaveResultAsync(userId, answers, selectedRole);

        var saved = await personalityTestRepository.GetByUserIdAsync(userId);
        Assert.Equal(selectedRole, saved!.SelectedRole);
    }

    [Fact]
    public async Task SaveResultAsync_ResultAlreadyExistsForUser_UpdatesExistingResultKeepingId()
    {
        personalityTestRepository.Seed(new PersonalityTestResult
        {
            PersonalityTestResultId = 7,
            User = new User { UserId = 1 },
            SelectedRole = JobRole.FrontendDeveloper,
        });
        var question = new Question { Trait = TraitType.Depth, SortOrder = 1 };
        var answers = new Dictionary<Question, AnswerValue> { [question] = AnswerValue.StronglyAgree };

        await service.SaveResultAsync(1, answers, JobRole.BackendDeveloper);

        var saved = await personalityTestRepository.GetByUserIdAsync(1);
        Assert.Equal(7, saved!.PersonalityTestResultId);
        Assert.Equal(JobRole.BackendDeveloper, saved.SelectedRole);
    }
}