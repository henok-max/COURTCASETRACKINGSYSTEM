using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    [Required]
    public required string Token { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Password must be at least 6 characters.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public required string ConfirmPassword { get; set; }
}
