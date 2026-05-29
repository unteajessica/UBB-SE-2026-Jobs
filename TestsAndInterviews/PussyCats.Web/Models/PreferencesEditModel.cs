using System.ComponentModel.DataAnnotations;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Web.Models;

public class PreferencesEditModel
{
    [Display(Name = "Preferred job roles (select 1–3)")]
    [MinLength(1, ErrorMessage = "Pick at least one role.")]
    [MaxLength(3, ErrorMessage = "Pick at most three roles.")]
    public List<JobRole> SelectedRoles { get; set; } = new();

    [Display(Name = "Work mode")]
    public WorkMode WorkMode { get; set; }

    [Display(Name = "Preferred location")]
    [StringLength(120)]
    public string Location { get; set; } = string.Empty;
}
