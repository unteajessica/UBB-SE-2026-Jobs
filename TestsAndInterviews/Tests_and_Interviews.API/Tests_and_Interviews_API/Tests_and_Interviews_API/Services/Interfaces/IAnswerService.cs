namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Defines operations for managing answers.
    /// </summary>
    public interface IAnswerService
    {
        /// <summary>
        /// Asynchronously saves the specified answer.
        /// </summary>
        /// <param name="answer">The answer to save. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SaveAsync(Answer answer);

        /// <summary>
        /// Asynchronously retrieves all answers associated with the specified attempt.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the attempt.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of answers for the specified attempt.</returns>
        Task<List<Answer>> FindByAttemptAsync(int attemptId);
    }
}