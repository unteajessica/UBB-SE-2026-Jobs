using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.PersonalityTests;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(int questionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Question>> GetAllOrderedAsync(CancellationToken cancellationToken = default);
}
