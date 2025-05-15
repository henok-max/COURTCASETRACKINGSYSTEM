using CourtCaseTrackingSystem.Models; // Add this
using System.Collections.Generic;

namespace CourtCaseTrackingSystem.Models
{
    public class DashboardViewModel
    {
        public int TodaysCaseCount { get; set; }
        public int PendingCaseCount { get; set; }
        public List<Case> RecentCases { get; set; } = new List<Case>();

    }
}