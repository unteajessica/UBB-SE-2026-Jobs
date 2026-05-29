namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Defines operations for managing test attempts.
    /// </summary>
    public interface ITestAttemptService
    {
        /// <summary>
        /// Asynchronously retrieves the test attempt with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the test attempt.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the test attempt, or null if not found.</returns>
        Task<TestAttempt?> FindByIdAsync(int id);

        /// <summary>
        /// Asynchronously retrieves the test attempt for the specified user and test.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the test attempt, or null if not found.</returns>
        Task<TestAttempt?> FindByUserAndTestAsync(int userId, int testId);

        /// <summary>
        /// Asynchronously saves a new test attempt to the data store.
        /// </summary>
        /// <param name="attempt">The test attempt to save. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SaveAsync(TestAttempt attempt);

        /// <summary>
        /// Asynchronously updates an existing test attempt in the data store.
        /// </summary>
        /// <param name="attempt">The test attempt with updated values. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated test attempt, or null if not found.</returns>
        Task<TestAttempt?> UpdateAsync(TestAttempt attempt);

        /// <summary>
        /// Asynchronously retrieves all valid completed attempts for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of valid test attempts for the specified test.</returns>
        Task<List<TestAttempt>> FindValidAttemptsByTestIdAsync(int testId);
    }
}