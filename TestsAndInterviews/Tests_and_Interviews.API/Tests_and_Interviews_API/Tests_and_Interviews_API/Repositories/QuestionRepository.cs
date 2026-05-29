namespace Tests_and_Interviews_API.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews_API.Data;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Models.Enums;
    using Tests_and_Interviews_API.Repositories.Interfaces;

    /// <summary>
    /// QuestionRepository class provides methods to perform CRUD operations on the Questions table in the database.
    /// </summary>
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionRepository"/> class.
        /// </summary>
        public QuestionRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        /// <inheritdoc />
        public async Task<List<Question>> FindByTestIdAsync(int testId)
        {
            return await this.appDbContext.Questions
                .Include(q => q.Answers)
                .Where(q => q.TestId == testId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Question>> GetInterviewQuestionsByPositionAsync(int positionId)
        {
            return await this.appDbContext.Questions
                .Where(q => q.QuestionTypeString == QuestionType.INTERVIEW.ToString()
                    && q.PositionId == positionId)
                .ToListAsync();
        }
    }
}