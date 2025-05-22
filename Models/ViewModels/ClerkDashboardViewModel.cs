using CourtCaseTrackingSystem.Models; // 👈 Add this line

namespace CourtCaseTrackingSystem.ViewModels
{
public class ClerkDashboardViewModel
{
    public int TotalCases { get; set; }
    public int PendingCases { get; set; }
    public int TodayRegisteredCases { get; set; }
    public List<Case> RecentCases { get; set; }
}
}