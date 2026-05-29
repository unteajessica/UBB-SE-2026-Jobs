namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// ITestRepostory interface provides methods to perform CRUD operations on the Tests and Questions.
    /// </summary>
    public interface ITestRepository
    {
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
    }
}
