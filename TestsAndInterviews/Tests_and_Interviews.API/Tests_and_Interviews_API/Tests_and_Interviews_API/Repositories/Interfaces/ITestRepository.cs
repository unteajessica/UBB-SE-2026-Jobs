namespace Tests_and_Interviews_API.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// ITestRepostory interface provides methods to perform CRUD operations on the Tests and Questions.
    /// </summary>
    public interface ITestRepository
    {
        /// <summary>
        /// Asynchronously retrieves all test entities from the data store.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all tests. The list is
        /// empty if no tests are found.</returns>

        public Task<List<Test>> GetTestsASync();

        /// <summary>
        /// Asynchronously retrieves a list of all question category names associated with tests.
        /// </summary>
        /// <returns>A list of strings containing the names of all categories for questions that are linked to a test. The list
        /// will be empty if no such categories exist.</returns>
        public Task<List<string>> GetAllCategories();

        /// <summary>
        /// Finds a test by its ID, including its associated questions.
        /// </summary>
        /// <param name="id">The ID of the test to find.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<Test?> FindByIdAsync(int id);

        /// <summary>
        /// Asynchronously finds tests by their category, including their associated questions.
        /// </summary>
        /// <param name="category">The category of the tests to find.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<Test>> FindTestsByCategoryAsync(string category);

        /// <summary>
        /// Asynchronously adds a new Test entity to the data store.
        /// </summary>
        /// <param name="test">The Test entity to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        public Task AddAsync(Test test);

        /// <summary>
        /// Asynchronously updates the specified test entity in the database.
        /// </summary>
        /// <param name="test">The test entity containing updated values. The entity's Id must correspond to an existing test in the
        /// database.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test with the specified Id does not exist in the database.</exception>
        public Task UpdateAsync(Test test);

        /// <summary>
        /// Asynchronously deletes the test entity with the specified identifier from the data store.
        /// </summary>
        /// <param name="id">The unique identifier of the test entity to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test entity with the specified identifier does not exist.</exception>
        public Task DeleteAsync(int id);
    }
}
