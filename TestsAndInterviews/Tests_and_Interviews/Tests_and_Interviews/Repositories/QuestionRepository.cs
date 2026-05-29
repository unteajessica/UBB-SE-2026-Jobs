namespace Tests_and_Interviews.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Tests_and_Interviews.Data;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Repositories.Interfaces;

    /// <summary>
    /// QuestionRepository class provides methods to perform CRUD operations on the Questions table in the database.
    /// </summary>
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionRepository"/> class.
        /// </summary>
        public QuestionRepository()
        {
            this.appDbContext = new AppDbContext();
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