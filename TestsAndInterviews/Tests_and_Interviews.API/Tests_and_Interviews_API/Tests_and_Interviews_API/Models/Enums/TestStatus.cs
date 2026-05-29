namespace Tests_and_Interviews_API.Models.Enums
{
    /// <summary>
    /// Specifies the status of a test within its lifecycle.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to track and manage the progression of a test through its various
    /// stages, such as not started, recording, submitted, and completed. The values represent distinct phases and can
    /// be used to control workflow or display status to users.
    /// </remarks>
    public enum TestStatus
    {
        /// <summary>
        /// Indicates that the operation has not started.
        /// </summary>
        AVAILABLE,

        /// <summary>
        /// Indicates id a test is in progress or not
        /// </summary>
        IN_PROGRESS,

        /// <summary>
        /// Represents the state when the test has been completed, indicating that all necessary actions have been finalized.
        /// </summary>
        COMPLETED,

        /// <summary>
        /// Indicates if a test was started, but not completed in due time.
        /// </summary>
        EXPIRED,
    }
}