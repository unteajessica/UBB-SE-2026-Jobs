using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PussyCats.Web.Models;

public class DocumentFormModel
{
    public int DocumentId { get; set; }

    [Required(ErrorMessage = "Please select a user.")]
    [Display(Name = "User")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Please enter a document name.")]
    [StringLength(100, ErrorMessage = "Document name cannot exceed 100 characters.")]
    [Display(Name = "Document Name")]
    public string DocumentName { get; set; } = string.Empty;

    [Display(Name = "Document File (JSON only for CV parsing)")]
    public IFormFile? File { get; set; }

    [Display(Name = "Parse as CV and update profile?")]
    public bool IsCv { get; set; }
}
