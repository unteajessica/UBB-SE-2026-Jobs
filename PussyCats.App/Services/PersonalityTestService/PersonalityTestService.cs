using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.PersonalityTests;

namespace PussyCats_App.Services.PersonalityTestService;

public class PersonalityTestService : IPersonalityTestService
{
    private readonly IPersonalityTestRepository personalityTestRepository;

    public PersonalityTestService(IPersonalityTestRepository personalityTestRepository)
    {
        this.personalityTestRepository = personalityTestRepository;
    }

    public static IReadOnlyList<Question> LoadQuestions()
    {
        int sortOrder = 0;
        Question Make(string text, TraitType trait) =>
            new Question { QuestionText = text, Trait = trait, SortOrder = ++sortOrder };

        var questions = new List<Question>();
        questions.AddRange(GetVisibilityTraitQuestions(Make));
        questions.AddRange(GetInteractionTraitQuestions(Make));
        questions.AddRange(GetDepthTraitQuestions(Make));
        questions.AddRange(GetCreativityTraitQuestions(Make));
        questions.AddRange(GetPaceTraitQuestions(Make));
        questions.AddRange(GetAbstractionTraitQuestions(Make));
        return questions.AsReadOnly();
    }

    private static List<Question> GetVisibilityTraitQuestions(Func<string, TraitType, Question> make)
    {
        return
        [
            make("I notice design details in apps and websites that most people would overlook.", TraitType.Visibility),
            make("I believe a seamless, high-quality user interface is just as critical to a project's success as the underlying code.", TraitType.Visibility),
            make("I believe that a project isn't truly 'finished' until the visual polish matches the technical quality.", TraitType.Visibility),
            make("I find myself drawn to tools and interfaces that are clean and well-designed.", TraitType.Visibility),
        ];
    }

    private static List<Question> GetInteractionTraitQuestions(Func<string, TraitType, Question> make)
    {
        return
        [
            make("I enjoy collaborating with others more than working through problems on my own.", TraitType.Interaction),
            make("I feel energized after meetings or group discussions rather than drained.", TraitType.Interaction),
            make("I would rather manage relationships and expectations than debug a technical issue.", TraitType.Interaction),
            make("I prefer roles where communication is a big part of the daily work.", TraitType.Interaction),
        ];
    }

    private static List<Question> GetDepthTraitQuestions(Func<string, TraitType, Question> make)
    {
        return
        [
            make("When something breaks, I want to understand exactly why — not just fix the surface issue.", TraitType.Depth),
            make("I enjoy reading documentation or technical material to fully understand a system.", TraitType.Depth),
            make("I find it satisfying to deeply master one topic rather than know a little about many.", TraitType.Depth),
            make("I get curious about what's happening \"behind the scenes\" in the tools and systems I use.", TraitType.Depth),
        ];
    }

    private static List<Question> GetCreativityTraitQuestions(Func<string, TraitType, Question> make)
    {
        return
        [
            make("I thrive when given a problem with no clear solution rather than a checklist to follow.", TraitType.Creativity),
            make("I enjoy coming up with new ideas more than executing someone else's plan.", TraitType.Creativity),
            make("I prefer work that leaves room for experimentation over work with strict rules and procedures.", TraitType.Creativity),
            make("I am most productive when tackling new problems rather than refining existing processes.", TraitType.Creativity),
        ];
    }

    private static List<Question> GetPaceTraitQuestions(Func<string, TraitType, Question> make)
    {
        return
        [
            make("I work best when I have several different tasks to switch between throughout the day.", TraitType.Pace),
            make("I enjoy fast-paced environments where priorities shift and I have to adapt quickly.", TraitType.Pace),
            make("I prefer having many smaller responsibilities over owning one large long-term problem.", TraitType.Pace),
            make("I feel productive when I can check off multiple different things in a single day.", TraitType.Pace),
        ];
    }

    private static List<Question> GetAbstractionTraitQuestions(Func<string, TraitType, Question> make)
    {
        return
        [
            make("I enjoy working with mathematical concepts, formulas, or statistical models.", TraitType.Abstraction),
            make("I find theoretical or abstract problems more interesting than purely practical ones.", TraitType.Abstraction),
            make("I am comfortable working with data, probabilities, and logical frameworks.", TraitType.Abstraction),
            make("I prefer to understand the logic and first principles of a system rather than just knowing how to operate it.", TraitType.Abstraction),
        ];
    }

    public IReadOnlyDictionary<TraitType, double> CalculateTraitScores(IReadOnlyDictionary<Question, AnswerValue> personalityTestAnswers)
    {
        var totalScorePerTrait = new Dictionary<TraitType, double>();
        var questionCountPerTrait = new Dictionary<TraitType, int>();

        foreach (var personalityTestAnswer in personalityTestAnswers)
        {
            var questionTrait = personalityTestAnswer.Key.Trait;

            if (!totalScorePerTrait.ContainsKey(questionTrait))
            {
                totalScorePerTrait[questionTrait] = 0;
                questionCountPerTrait[questionTrait] = 0;
            }

            totalScorePerTrait[questionTrait] += (int)personalityTestAnswer.Value;
            questionCountPerTrait[questionTrait]++;
        }

        foreach (var trait in totalScorePerTrait.Keys)
        {
            totalScorePerTrait[trait] /= questionCountPerTrait[trait];
        }

        return totalScorePerTrait;
    }

    public IReadOnlyDictionary<JobRole, double> CalculateRoleScores(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        var roleScores = new Dictionary<JobRole, double>();
        roleScores.Add(JobRole.FrontendDeveloper, CalculateFrontend(traitScores));
        roleScores.Add(JobRole.BackendDeveloper, CalculateBackend(traitScores));
        roleScores.Add(JobRole.UiUxDesigner, CalculateUIUX(traitScores));
        roleScores.Add(JobRole.DevOpsEngineer, CalculateDevOps(traitScores));
        roleScores.Add(JobRole.ProjectManager, CalculateProjectManager(traitScores));
        roleScores.Add(JobRole.DataAnalyst, CalculateDataAnalyst(traitScores));
        roleScores.Add(JobRole.CybersecuritySpecialist, CalculateCyberSecurity(traitScores));
        roleScores.Add(JobRole.AiMlEngineer, CalculateAIEngineer(traitScores));
        return roleScores;
    }

    public IReadOnlyDictionary<JobRole, double> GetTopRoles(IReadOnlyDictionary<JobRole, double> roleScores, int count)
    {
        return roleScores
            .OrderByDescending(roleWithScore => roleWithScore.Value)
            .Take(count)
            .ToDictionary(roleWithScore => roleWithScore.Key, roleWithScore => roleWithScore.Value);
    }

    public async Task SaveResultAsync(int userId, IReadOnlyDictionary<Question, AnswerValue> answers, JobRole selectedRole, CancellationToken cancellationToken = default)
    {
        var traitScores = CalculateTraitScores(answers);

        var traitScoreEntities = traitScores
            .Select(traitWithScore => new PersonalityTraitScore
            {
                Trait = traitWithScore.Key,
                Score = (int)Math.Round(traitWithScore.Value),
            })
            .ToList();

        var newResult = new PersonalityTestResult
        {
            User = new User { UserId = userId },
            CompletedAt = DateTime.UtcNow,
            SelectedRole = selectedRole,
            TraitScores = traitScoreEntities,
        };

        var existingResult = await personalityTestRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (existingResult is null)
        {
            await personalityTestRepository.AddAsync(newResult, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            newResult.PersonalityTestResultId = existingResult.PersonalityTestResultId;
            await personalityTestRepository.UpdateAsync(newResult, cancellationToken).ConfigureAwait(false);
        }
    }

    private double CalculateFrontend(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int visibilityWeight = 2;
        const int creativiyWeight = 2;
        return (traitScores[TraitType.Visibility] * visibilityWeight) +
               (traitScores[TraitType.Creativity] * creativiyWeight) +
               traitScores[TraitType.Pace];
    }

    private double CalculateBackend(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int depthWeight = 2;
        const int visibilityWeight = 2;
        const int baselineForVisibility = 5;
        return (traitScores[TraitType.Depth] * depthWeight) +
               ((baselineForVisibility - traitScores[TraitType.Visibility]) * visibilityWeight) +
               traitScores[TraitType.Pace];
    }

    private double CalculateUIUX(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int visibilityWeight = 3;
        const int creativityWeight = 2;
        return (traitScores[TraitType.Visibility] * visibilityWeight) +
               (traitScores[TraitType.Creativity] * creativityWeight) +
               traitScores[TraitType.Interaction];
    }

    private double CalculateDevOps(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int depthWeight = 2;
        const int paceWeight = 2;
        const int baselineForInteraction = 5;
        return (traitScores[TraitType.Depth] * depthWeight) +
               (traitScores[TraitType.Pace] * paceWeight) +
               (baselineForInteraction - traitScores[TraitType.Interaction]);
    }

    private double CalculateProjectManager(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int interactionWeight = 3;
        const int baselineForDepth = 5;
        return (traitScores[TraitType.Interaction] * interactionWeight) +
               traitScores[TraitType.Creativity] +
               (baselineForDepth - traitScores[TraitType.Depth]);
    }

    private double CalculateDataAnalyst(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int depthWeight = 2;
        const int abstractionWeight = 2;
        const int baselineForInteraction = 5;
        return (traitScores[TraitType.Depth] * depthWeight) +
               (traitScores[TraitType.Abstraction] * abstractionWeight) +
               (baselineForInteraction - traitScores[TraitType.Interaction]);
    }

    private double CalculateCyberSecurity(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int depthWeight = 3;
        const int baselineForInteraction = 6;
        const int baselineForPace = 6;
        return (traitScores[TraitType.Depth] * depthWeight) +
               (baselineForInteraction - traitScores[TraitType.Interaction]) +
               (baselineForPace - traitScores[TraitType.Pace]);
    }

    private double CalculateAIEngineer(IReadOnlyDictionary<TraitType, double> traitScores)
    {
        const int depthWeight = 3;
        const int abstractionWeight = 2;
        return (traitScores[TraitType.Depth] * depthWeight) +
               traitScores[TraitType.Creativity] +
               (traitScores[TraitType.Abstraction] * abstractionWeight);
    }
}
