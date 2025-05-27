using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CourtCaseTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using CourtCaseTrackingSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CourtCaseTrackingSystem.Controllers;

public class HomeController : Controller
{
    private readonly CourtDbContext _context;
    private readonly ILogger<HomeController> _logger;

    // Corrected single constructor
    public HomeController(CourtDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult PublicPortal()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> TrackCase(string caseNumber)
    {
        var caseItem = await _context.Cases
            .Select(c => new
            {
                c.CaseNumber,
                c.Title,
                c.PlaintiffName,
                c.DefendantName,
                c.Status,
                c.HearingDateTime,
                c.AppointmentDateTime,
                c.RegistrationDate
            })
            .FirstOrDefaultAsync(c => c.CaseNumber == caseNumber);

        if (caseItem == null)
        {
            ModelState.AddModelError("", "Case not found");
            return View("PublicPortal");
        }

        var result = new PublicCaseViewModel
        {
            CaseNumber = caseItem.CaseNumber,
            Title = caseItem.Title,
            PlaintiffName = caseItem.PlaintiffName,
            DefendantName = caseItem.DefendantName,
            Status = caseItem.Status,
            RegistrationDate = caseItem.RegistrationDate,
            UpcomingHearing = caseItem.HearingDateTime > DateTime.UtcNow ? caseItem.HearingDateTime : null,
            AppointmentDate = caseItem.AppointmentDateTime > DateTime.UtcNow ? caseItem.AppointmentDateTime : null
        };

        return View("_PublicCaseResult", result);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}