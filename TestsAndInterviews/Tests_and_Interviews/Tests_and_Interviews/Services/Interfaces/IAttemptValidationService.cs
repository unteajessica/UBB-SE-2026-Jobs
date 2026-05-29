namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// IAttemptValidationService interface provides methods to validate test attempts, including checking for existing attempts and determining if a user can start a new test attempt.
    /// </summary>
    public interface IAttemptValidationService
    {
        /// <summary>
        /// Asynchronously checks if a user can start a test attempt by verifying if there are
        /// any existing attempts for the given user and test.
        /// </summary>
        /// <param name="userId">The ID of the user attempting to start the test.</param>
        /// <param name="testId">The ID of the test the user is attempting to start.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<bool> CanStartTestAsync(int userId, int testId);

        /// <summary>
        /// Asynchronously checks for existing test attempts for a given user and test,
        /// and throws an exception if an attempt already exists.
        /// </summary>
        /// <param name="userId">The ID of the user attempting to start the test.</param>
        /// <param name="testId">The ID of the test the user is attempting to start.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when an existing attempt is found for the user and test.</exception>
        Task CheckExistingAttemptsAsync(int userId, int testId);
    }
}
