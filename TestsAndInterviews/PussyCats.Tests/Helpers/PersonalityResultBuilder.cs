using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Tests.Helpers;

public class PersonalityResultBuilder
{
    private int personalityTestResultId = 1;
    private int userId = 1;
    private DateTime completedAt = DateTime.UtcNow;
    private JobRole? selectedRole;
    private readonly List<PersonalityTraitScore> traitScores = new();

    public PersonalityResultBuilder WithId(int id)
    {
        personalityTestResultId = id;
        return this;
    }

    public PersonalityResultBuilder ForUser(int id)
    {
        userId = id;
        return this;
    }

    public PersonalityResultBuilder CompletedAt(DateTime value)
    {
        completedAt = value;
        return this;
    }

    public PersonalityResultBuilder WithSelectedRole(JobRole? value)
    {
        selectedRole = value;
        return this;
    }

    public PersonalityResultBuilder WithTraitScore(TraitType trait, int score)
    {
        traitScores.Add(new PersonalityTraitScore
        {
            PersonalityTestResult = new PersonalityTestResult { PersonalityTestResultId = personalityTestResultId },
            Trait = trait,
            Score = score,
        });
        return this;
    }

    public PersonalityTestResult Build()
    {
        foreach (var traitScore in traitScores)
        {
            traitScore.PersonalityTestResult = new PersonalityTestResult { PersonalityTestResultId = personalityTestResultId };
        }
        return new PersonalityTestResult
        {
            PersonalityTestResultId = personalityTestResultId,
            User = new User { UserId = userId},
            CompletedAt = completedAt,
            SelectedRole = selectedRole,
            TraitScores = traitScores,
        };
    }
}
