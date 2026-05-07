using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.PersonalityTests;

namespace PussyCats.App.RepositoryProxies;

public class QuestionRepositoryProxy : IQuestionRepository
{
    public QuestionRepositoryProxy(HttpClient http)
    {
        _ = http;
    }

    public Task<Question?> GetByIdAsync(int questionId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Questions are hardcoded in PersonalityTestService; see MergePlan.md section 4.");
    }

    public Task<IReadOnlyList<Question>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Questions are hardcoded in PersonalityTestService; see MergePlan.md section 4.");
    }
}
