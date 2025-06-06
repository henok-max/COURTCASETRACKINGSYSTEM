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

       [Required(ErrorMessage = "Please upload a document.")]
       [Display(Name = "Document")]
        public IFormFile Document { get; set; }
        public IEnumerable<SelectListItem> CaseTypeOptions { get; set; } = new List<SelectListItem>();

        [Display(Name = "NewDocument")]
        public IFormFile? NewDocument { get; set; }

        [Phone]
         public string? PlaintiffPhone { get; set; }

        [Phone]
        public string? DefendantPhone { get; set; }

        [Display(Name = "Send SMS Notifications")]
        public bool SmsNotificationsEnabled { get; set; } = true;
    }
}