using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.PersonalityTests;

public interface IPersonalityTestRepository
{
    Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken cancellationToken = default);

    Task UpdateAsync(PersonalityTestResult result, CancellationToken cancellationToken = default);

    Task RemoveAsync(int personalityTestResultId, CancellationToken cancellationToken = default);
}
