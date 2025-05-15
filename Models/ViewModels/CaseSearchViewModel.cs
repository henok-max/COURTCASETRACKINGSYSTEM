using CourtCaseTrackingSystem.Models; // Add this
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace CourtCaseTrackingSystem.ViewModels
{
    public class CaseSearchViewModel
    {
        public List<Case> Cases { get; set; } = new List<Case>();
        public string? SearchString { get; set; }
        public string? SearchType { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<SelectListItem> SearchTypes => new List<SelectListItem>
        {
            new SelectListItem { Text = "Case Number", Value = "CaseNumber" },
            new SelectListItem { Text = "Case Title", Value = "Title" },
            new SelectListItem { Text = "Plaintiff Name", Value = "Plaintiff" },
            new SelectListItem { Text = "Defendant Name", Value = "Defendant" },
            new SelectListItem { Text = "Case Status", Value = "Status" }
        };
    }
}