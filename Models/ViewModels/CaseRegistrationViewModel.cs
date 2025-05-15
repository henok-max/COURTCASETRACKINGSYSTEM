using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using CourtCaseTrackingSystem.Models;

namespace CourtCaseTrackingSystem.ViewModels
{
    public class CaseRegistrationViewModel
    {
        // Option 1: Use nullable types with [Required]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plaintiff name is required")]
        public string PlaintiffName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Defendant name is required")]
        public string DefendantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wereda is required")]
        public string Wereda { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Case type is required")]
        public CaseType SelectedCaseType { get; set; }

        [Display(Name = "Document")]
        public IFormFile? Document { get; set; }
        public string? Description { get; set; } // Optional
        public IEnumerable<SelectListItem> CaseTypeOptions { get; set; } = new List<SelectListItem>();
    }
}