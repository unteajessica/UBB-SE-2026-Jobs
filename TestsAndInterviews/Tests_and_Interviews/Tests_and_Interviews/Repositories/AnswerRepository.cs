namespace Tests_and_Interviews.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Data;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Repositories.Interfaces;

    /// <summary>
    /// AnswerRepository class provides methods to perform CRUD operations on the Answers table in the database.
    /// </summary>
    public class AnswerRepository : IAnswerRepository
    {
        private readonly AppDbContext appDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnswerRepository"/> class.
        /// </summary>
        public AnswerRepository()
        {
            this.appDbContext = new AppDbContext();
        }

        /// <inheritdoc/>
        public async Task SaveAsync(Answer answer)
        {
            this.appDbContext.Answers.Add(answer);
            await this.appDbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Answer>> FindByAttemptAsync(int attemptId)
        {
            return await this.appDbContext.Answers
                .Include(a => a.Question)
                .Where(a => a.AttemptId == attemptId)
                .ToListAsync();
        }
    }
}