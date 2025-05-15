using System.ComponentModel.DataAnnotations;

namespace CourtCaseTrackingSystem.ViewModels
{
    public class CaseConfirmationViewModel
    {
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; } = string.Empty;

        [Display(Name = "Registration Time")]
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Assigned Judge")]
        public string AssignedJudge { get; set; } = string.Empty;

        [Display(Name = "Document Reference")]
        public string DocumentReference { get; set; } = string.Empty;
    }
}