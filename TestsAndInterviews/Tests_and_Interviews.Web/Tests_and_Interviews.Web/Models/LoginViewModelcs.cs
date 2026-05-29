namespace Tests_and_Interviews.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// View model for the login page.
    /// </summary>
    public class LoginViewModel
    {
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
        public string Password { get; set; } = string.Empty;
    }
}