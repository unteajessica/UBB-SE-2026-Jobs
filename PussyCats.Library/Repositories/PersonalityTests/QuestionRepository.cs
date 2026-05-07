using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.PersonalityTests;

public class QuestionRepository : IQuestionRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public QuestionRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    public async Task<Question?> GetByIdAsync(int questionId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(question => question.QuestionId == questionId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the catalog ordered by SortOrder so the UI can render questions in the original
    /// designer-defined sequence. Read-only.
    /// </summary>
    public async Task<IReadOnlyList<Question>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Questions
            .AsNoTracking()
            .OrderBy(question => question.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
