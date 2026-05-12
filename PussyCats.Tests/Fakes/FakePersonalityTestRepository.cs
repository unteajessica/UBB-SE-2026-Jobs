using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.PersonalityTests;

namespace PussyCats.Tests.Fakes;

public class FakePersonalityTestRepository : IPersonalityTestRepository
{
    private readonly Dictionary<int, PersonalityTestResult> personalityTestResultsById = new();

    public void Seed(params PersonalityTestResult[] results)
    {
        foreach (var result in results)
        {
            personalityTestResultsById[result.PersonalityTestResultId] = result;
        }
    }

    public Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var result = personalityTestResultsById.Values.FirstOrDefault(personalityTestResult => personalityTestResult.User.UserId == userId);
        return Task.FromResult(result);
    }

    public Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        if (result.PersonalityTestResultId == 0)
        {
            result.PersonalityTestResultId = NextId();
        }
        personalityTestResultsById[result.PersonalityTestResultId] = result;
        return Task.FromResult(result);
    }

    public Task UpdateAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        personalityTestResultsById[result.PersonalityTestResultId] = result;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int personalityTestResultId, CancellationToken cancellationToken = default)
    {
        personalityTestResultsById.Remove(personalityTestResultId);
        return Task.CompletedTask;
    }

    private int NextId() => personalityTestResultsById.Count == 0 ? 1 : personalityTestResultsById.Keys.Max() + 1;
}
