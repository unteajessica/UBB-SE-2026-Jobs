namespace Tests_and_Interviews_API.DTOs
{
    /// <summary>
    /// Returned to the MVC app after successful login or register.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Gets or sets the JWT token string.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user's company id (if they are a recruiter).
        /// </summary>
        public int? CompanyId { get; set; }
    }
}