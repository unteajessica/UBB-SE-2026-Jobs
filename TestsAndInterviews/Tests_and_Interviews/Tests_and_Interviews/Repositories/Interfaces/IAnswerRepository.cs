namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// IAnswerRepository interface provides methods that perform CRUD operations on the Answer data.
    /// </summary>
    public interface IAnswerRepository
    {
        /// <summary>
        /// Asynchronously saves an answer to the database. It inserts a new record into the Answers table with the provided answer details.
        /// </summary>
        /// <param name="answer">The <see cref="Answer"/> object containing the details of the answer to be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveAsync(Answer answer);

        /// <summary>
        /// Asynchronously retrieves a list of answers associated with a specific attempt ID from the database.
        /// It performs a JOIN operation between the Answers and Questions tables to fetch the relevant answer
        /// details along with the corresponding question information.
        /// </summary>
        /// <param name="attemptId">The ID of the attempt for which to retrieve the answers.</param>
        /// <returns>A <see cref="Task{List{Answer}}"/> representing the asynchronous operation, containing a list of <see cref="Answer"/> objects.</returns>
        Task<List<Answer>> FindByAttemptAsync(int attemptId);
    }
}
