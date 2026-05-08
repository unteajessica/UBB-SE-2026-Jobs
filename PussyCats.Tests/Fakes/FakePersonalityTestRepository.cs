using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.PersonalityTests;

namespace PussyCats.Tests.Fakes;

public class FakePersonalityTestRepository : IPersonalityTestRepository
{
    private readonly Dictionary<int, PersonalityTestResult> store = new();

    public void Seed(params PersonalityTestResult[] results)
    {
        foreach (var result in results)
        {
            store[result.PersonalityTestResultId] = result;
        }
    }

    public Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var result = store.Values.FirstOrDefault(personalityTestResult => personalityTestResult.UserId == userId);
        return Task.FromResult(result);
    }

    public Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        if (result.PersonalityTestResultId == 0)
        {
            result.PersonalityTestResultId = NextId();
        }
        store[result.PersonalityTestResultId] = result;
        return Task.FromResult(result);
    }

    public Task UpdateAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        store[result.PersonalityTestResultId] = result;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int personalityTestResultId, CancellationToken cancellationToken = default)
    {
        store.Remove(personalityTestResultId);
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
