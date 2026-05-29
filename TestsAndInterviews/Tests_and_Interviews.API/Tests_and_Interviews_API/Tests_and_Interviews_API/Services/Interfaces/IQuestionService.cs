namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Defines operations for managing questions.
    /// </summary>
    public interface IQuestionService
    {
        /// <summary>
        /// Asynchronously retrieves all questions belonging to the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of questions for the specified test.</returns>
        Task<List<Question>> GetQuestionsByTestIdAsync(int testId);

        /// <summary>
        /// Asynchronously retrieves all interview questions for the specified position.
        /// </summary>
        /// <param name="positionId">The unique identifier of the position.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of interview questions for the specified position.</returns>
        Task<List<Question>> GetInterviewQuestionsByPositionAsync(int positionId);
    }
}