using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.PersonalityTests;

namespace PussyCats.Tests.Fakes;

public class FakeQuestionRepository : IQuestionRepository
{
    private readonly Dictionary<int, Question> store = new();

    public void Seed(params Question[] questions)
    {
        foreach (var question in questions)
        {
            store[question.QuestionId] = question;
        }
    }

    public Task<Question?> GetByIdAsync(int questionId, CancellationToken ct = default)
    {
        store.TryGetValue(questionId, out var question);
        return Task.FromResult(question);
    }

    public Task<IReadOnlyList<Question>> GetAllOrderedAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Question> ordered = store.Values.OrderBy(q => q.SortOrder).ToList();
        return Task.FromResult(ordered);
    }
}
