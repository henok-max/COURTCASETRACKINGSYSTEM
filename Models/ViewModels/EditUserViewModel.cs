using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EditUserViewModel
{
    public required string Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public string SelectedRole { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Status")]
    public required string Status { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    // Keep only ONE RoleOptions declaration
    public List<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>
    {
        new SelectListItem { Value = "Admin", Text = "Admin" },
        new SelectListItem { Value = "Judge", Text = "Judge" },
        new SelectListItem { Value = "Clerk", Text = "Clerk" },
        new SelectListItem { Value = "Registrar", Text = "Registrar" }
    };

    public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
    {
        new SelectListItem { Value = "Active", Text = "Active" },
        new SelectListItem { Value = "Inactive", Text = "Inactive" }
    };
}