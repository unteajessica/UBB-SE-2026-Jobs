namespace Tests_and_Interviews_API.Dtos
{
    /// <summary>
    /// Data transfer object for starting a test attempt.
    /// </summary>
    public class StartTestDto
    {
        /// <summary>Gets or sets the user identifier.</summary>
        public int UserId { get; set; }

        /// <summary>Gets or sets the test identifier.</summary>
        public int TestId { get; set; }
    }
}