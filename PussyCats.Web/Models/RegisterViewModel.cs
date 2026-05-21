using System.ComponentModel.DataAnnotations;

namespace PussyCats.Web.Models;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Prenume")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Nume")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmă parola")]
    [Compare(nameof(Password), ErrorMessage = "Parolele nu coincid.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
