namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing test attempts.
    /// </summary>
    public class TestAttemptService : ITestAttemptService
    {
        private readonly ITestAttemptRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAttemptService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access test attempt data. Cannot be null.</param>
        public TestAttemptService(ITestAttemptRepository repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Asynchronously retrieves the test attempt with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the test attempt.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the test attempt, or null if not found.</returns>
        public async Task<TestAttempt?> FindByIdAsync(int id)
        {
            return await this._repository.FindByIdAsync(id);
        }

        /// <summary>
        /// Asynchronously retrieves the test attempt for the specified user and test.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the test attempt, or null if not found.</returns>
        public async Task<TestAttempt?> FindByUserAndTestAsync(int userId, int testId)
        {
            return await this._repository.FindByUserAndTestAsync(userId, testId);
        }

        /// <summary>
        /// Asynchronously saves a new test attempt to the data store.
        /// </summary>
        /// <param name="attempt">The test attempt to save. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SaveAsync(TestAttempt attempt)
        {
            await this._repository.SaveAsync(attempt);
        }

        /// <summary>
        /// Asynchronously updates an existing test attempt in the data store.
        /// </summary>
        /// <param name="attempt">The test attempt with updated values. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated test attempt, or null if not found.</returns>
        public async Task<TestAttempt?> UpdateAsync(TestAttempt attempt)
        {
            return await this._repository.UpdateAsync(attempt);
        }

        /// <summary>
        /// Asynchronously retrieves all valid completed attempts for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of valid test attempts for the specified test.</returns>
        public async Task<List<TestAttempt>> FindValidAttemptsByTestIdAsync(int testId)
        {
            return await this._repository.FindValidAttemptsByTestIdAsync(testId);
        }
    }
}