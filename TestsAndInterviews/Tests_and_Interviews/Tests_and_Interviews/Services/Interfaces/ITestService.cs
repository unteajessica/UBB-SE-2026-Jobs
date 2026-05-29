namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Dtos;

    /// <summary>
    /// ITestService interface provides methods to manage the lifecycle of test attempts, including starting a test, submitting a test, and retrieving the next available test for a given category.
    /// </summary>
    public interface ITestService
    {
        /// <summary>
        /// Asynchronously starts a test attempt for a given user and test. It checks for existing attempts,
        /// </summary>
        /// <param name="userId">The ID of the user starting the test.</param>
        /// <param name="testId">The ID of the test to be attempted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StartTestAsync(int userId, int testId);

        /// <summary>
        /// Asynchronously submits a test attempt by grading the answers and calculating the final score. It retrieves
        /// </summary>
        /// <param name="attemptId">The ID of the test attempt to be submitted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SubmitTestAsync(int attemptId);

        /// <summary>
        /// Asynchronously submits a test attempt for a given user and test using the provided answers. The method
        /// persists answers, finalizes the attempt, processes the finalized attempt and returns the final score.
        /// </summary>
        /// <param name="userId">The ID of the user submitting the attempt.</param>
        /// <param name="testId">The ID of the test being submitted.</param>
        /// <param name="answers">The collection of answers provided by the user.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation that returns the final score as a float.</returns>
        Task<float> SubmitAttemptAsync(int userId, int testId, IEnumerable<AnswerDto> answers);

        /// <summary>
        /// Asynchronously retrieves the next available test for a given category. It queries the test repository for tests
        /// </summary>
        /// <param name="category">The category of the test to retrieve.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<Test?> GetNextAvailableTestAsync(string category);
        Task<List<Test>> FindTestsByCategoryAsync(string category);
        Task<Test> FindByIdAsync(int id);
    }
}
