using System.ComponentModel.DataAnnotations;

namespace CourtCaseTrackingSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username or Email")]
        public required string UsernameOrEmail { get; set; }



        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}