// ViewModels/JudgeDashboardViewModel.cs
using CourtCaseTrackingSystem.Models; // 👈 Add this line

namespace CourtCaseTrackingSystem.ViewModels
{
    public class JudgeDashboardViewModel
    {
        public int TotalCases { get; set; }
        public int PendingCases { get; set; }
        public List<Case> RecentCases { get; set; } = new List<Case>(); // Initialized collection
    }

}