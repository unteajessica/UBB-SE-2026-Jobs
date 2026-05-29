// <copyright file="ITestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Defines operations for managing tests.
    /// </summary>
    public interface ITestService
    {
        /// <summary>
        /// Asynchronously retrieves all test entities from the data store.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all test entities. The
        /// list will be empty if no tests are found.</returns>
        public Task<List<Test>> GetAll();

        /// <summary>
        /// Asynchronously retrieves a list of all available category names.
        /// </summary>
        /// <returns>A list of strings containing the names of all categories. The list is empty if no categories are found.</returns>
        public Task<List<string>> GetCategories();

        /// <summary>
        /// Asynchronously retrieves the test with the specified identifier, including its associated questions.
        /// </summary>
        /// <param name="id">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the test, or null if not found.</returns>
        Task<Test?> FindByIdAsync(int id);

        /// <summary>
        /// Asynchronously retrieves all tests belonging to the specified category, including their associated questions.
        /// </summary>
        /// <param name="category">The category to filter tests by.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of tests in the specified category.</returns>
        Task<List<Test>> FindTestsByCategoryAsync(string category);

        /// <summary>
        /// Asynchronously adds a new test entity to the data store.
        /// </summary>
        /// <param name="test">The test entity to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added test entity.</returns>
        public Task<Test> AddTestASync(Test test);

        /// <summary>
        /// Asynchronously updates an existing test with the specified identifier using the provided test data.
        /// </summary>
        /// <param name="id">The unique identifier of the test to update.</param>
        /// <param name="test">The test data to apply to the existing test. The <see cref="Test.Id"/> property is ignored and will be set
        /// to the specified <paramref name="id"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Test"/>
        /// instance.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test with the specified <paramref name="id"/> does not exist.</exception>
        public Task<Test> UpdateTestAsync(int id, Test test);

        /// <summary>
        /// Asynchronously deletes the test with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the test to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the test was
        /// successfully deleted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test with the specified <paramref name="id"/> does not exist.</exception>
        public Task<bool> DeleteTestAsync(int id);
        /// <summary>
        /// Starts a test attempt for the specified user and test.
        /// </summary>
        Task StartTestAsync(int userId, int testId);

        /// <summary>
        /// Submits a test attempt by grading all answers and calculating the final score.
        /// </summary>
        Task SubmitTestAsync(int attemptId);

        /// <summary>
        /// Submits a full attempt with answers, grades it, and returns the final score.
        /// </summary>
        Task<float> SubmitAttemptAsync(int userId, int testId, IEnumerable<AnswerDto> answers);
    }
}