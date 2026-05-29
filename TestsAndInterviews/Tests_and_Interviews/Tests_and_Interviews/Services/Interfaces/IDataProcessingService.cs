namespace Tests_and_Interviews.Services.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Orchestrates the validation and scoring of finalized test attempts.
    /// Ensures only eligible data is promoted to the leaderboard.
    /// </summary>
    public interface IDataProcessingService
    {
        /// <summary>
        /// Processes a raw test attempt by validating its metadata and calculating a final score.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the attempt to process.</param>
        /// <returns>
        /// <c>true</c> if the attempt was successfully validated and scored; 
        /// <c>false</c> if the attempt was rejected or not found.
        /// </returns>
        Task<bool> ProcessFinalizedAttemptAsync(int attemptId);
    }
}
