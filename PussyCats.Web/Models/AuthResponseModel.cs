namespace PussyCats.Web.Models
{
    /// <summary>
    /// Represents the authentication response received from the API.
    /// </summary>
    public class AuthResponseModel
    {
        /// <summary>
        /// Gets or sets the JWT token.
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
        /// Gets or sets the user's unique identifier.
        /// </summary>
        public int UserId { get; set; }
    }
}
