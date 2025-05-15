using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtCaseTrackingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string SecondName { get; set; }
        [Required]
        [EmailAddress]
        public override string? Email
        {
            get => base.Email ?? throw new InvalidOperationException("Email not initialized");
            set => base.Email = value ?? throw new ArgumentNullException(nameof(value));
        }
        public string Status { get; set; } = "Active";
        public int ActiveCaseCount { get; set; }
        public DateTime? LastAssignmentDate { get; set; }
        public DateTime? LastLoginDate { get; set; }



    }
}