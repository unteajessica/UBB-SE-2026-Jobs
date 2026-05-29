namespace Tests_and_Interviews_API.DTOs
{
    /// <summary>
    /// Data transfer object for registration requests.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plain-text password.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role: Candidate or Recruiter.
        /// </summary>
        public string Role { get; set; } = "Candidate";

        /// <summary>
        /// Gets or sets the company ID for Recruiter role registration.
        /// This is required when Role is "Recruiter", optional for "Candidate".
        /// </summary>
        public int? CompanyId { get; set; }
    }
}