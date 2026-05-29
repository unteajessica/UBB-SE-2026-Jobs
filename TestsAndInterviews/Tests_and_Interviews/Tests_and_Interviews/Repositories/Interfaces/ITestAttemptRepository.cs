namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// ITestAttemptRepository interface provides methods to perform CRUD operations on the TestAttempts.
    /// </summary>
    public interface ITestAttemptRepository
    {
        /// <summary>
        /// Asynchronously finds a test attempt by user ID and test ID.
        /// This method retrieves the test attempt along with its associated answers for a specific user and test.
        /// It returns a TestAttempt object if found, or null if no matching record exists.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to find the attempt.</param>
        /// <param name="testId">The ID of the test for which to find the attempt.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<TestAttempt?> FindByUserAndTestAsync(int userId, int testId);

        /// <summary>
        /// Asynchronously saves a new test attempt.
        /// </summary>
        /// <param name="attempt">The <see cref="TestAttempt"/> object to be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveAsync(TestAttempt attempt);

        /// <summary>
        /// Asynchronously updates an existing test attempt in the database.
        /// </summary>
        /// <param name="attempt">The <see cref="TestAttempt"/> object containing the updated data.
        /// The Id property must be set to identify which record to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<TestAttempt?> UpdateAsync(TestAttempt attempt);

        /// <summary>
        /// Asynchronously finds a test attempt by its ID.
        /// </summary>
        /// <param name="id">The id of the test attempt we want to find.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<TestAttempt?> FindByIdAsync(int id);

        /// <summary>
        /// Asynchronously finds all valid test attempts for a given test ID.
        /// </summary>
        /// <param name="testId">The ID of the test for which to find valid attempts.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<TestAttempt>> FindValidAttemptsByTestIdAsync(int testId);
    }
}
