using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Services.PersonalityTestService;

/// <summary>Personality trait scoring, role recommendation, and result persistence.</summary>
public interface IPersonalityTestService
{
    IReadOnlyDictionary<TraitType, double> CalculateTraitScores(IReadOnlyDictionary<Question, AnswerValue> answers);

    IReadOnlyDictionary<JobRole, double> CalculateRoleScores(IReadOnlyDictionary<TraitType, double> traitScores);

    IReadOnlyDictionary<JobRole, double> GetTopRoles(IReadOnlyDictionary<JobRole, double> roleScores, int count);

    Task SaveResultAsync(int userId, IReadOnlyDictionary<Question, AnswerValue> answers, JobRole selectedRole, CancellationToken cancellationToken = default);
}
