namespace Tests_and_Interviews.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// View model for the registration page.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's selected role.
        /// </summary>
        [Required]
        public string Role { get; set; } = "Candidate";

        /// <summary>
        /// Gets or sets the selected company ID for Recruiter registrations.
        /// </summary>
        public int? CompanyId { get; set; }
    }
}