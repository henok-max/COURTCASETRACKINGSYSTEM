using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtCaseTrackingSystem.Models

{
  public class Case
  {
    public int CaseID { get; set; }

    [Required]
    public required string CaseNumber { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public CaseType CaseType { get; set; }

    [Required]
    [StringLength(100)]
    public required string Wereda { get; set; }

    [Required]
    public required string City { get; set; }

    [Required]
    public required string PlaintiffName { get; set; }

    [Required]
    public required string DefendantName { get; set; }

    public string? DocumentPath { get; set; } // Make nullable
    public string? Description { get; set; }

    public string Status { get; set; } = "Pending";
    [Column(TypeName = "datetime2")]
    public DateTime? SummonDate { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? DecisionComments { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    public string? SummonLetterPath { get; set; }
    public DateTime? LastUpdated { get; set; }
    public DateTime? LastModified { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;  // Changed to UTC
    [Phone]
    [Display(Name = "Plaintiff Phone")]
    public string? PlaintiffPhone { get; set; }

    [Phone]
    [Display(Name = "Defendant Phone")]
    public string? DefendantPhone { get; set; }

    public bool PlaintiffSmsOptIn { get; set; } = true;
    public bool DefendantSmsOptIn { get; set; } = true;


    // Enum list storage
    public string? DefenseDocumentPath { get; set; }
    public DateTime? DefenseDocumentDate { get; set; }

    public string? WitnessDocumentPath { get; set; }
    public DateTime? WitnessDocumentDate { get; set; }

    public string? NewDocumentPath { get; set; }
    public DateTime? NewDocumentDate { get; set; }

    [Display(Name = "Hearing Date/Time")]
    [Column(TypeName = "datetime2")] // SQL Server data type
    public DateTime? HearingDateTime { get; set; }
    [Display(Name = "Appointment Date/Time")]
    [Column(TypeName = "datetime2")]
    public DateTime? AppointmentDateTime { get; set; } // Can be updated many times
    public string? Plea { get; set; }
    public string? JudgmentHistory { get; set; } // JSON array storage
    public string? DecreeHistory { get; set; }    // JSON array storage  
    [NotMapped] // Not stored in DB
    public string? NewJudgment { get; set; }

    [NotMapped] // Not stored in DB
    public string? NewDecree { get; set; }
    // Navigation properties
    [ForeignKey("AssignedJudge")]
    public required string AssignedJudgeId { get; set; }

    [Required]
    public required ApplicationUser AssignedJudge { get; set; }
  }
}