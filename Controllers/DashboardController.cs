using CourtCaseTrackingSystem.Models;
using CourtCaseTrackingSystem.Data;
using CourtCaseTrackingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CourtCaseTrackingSystem.Controllers
{
    public class DashboardController : Controller
    {
        // Role constants
        public const string AdminRole = "Admin";
        public const string JudgeRole = "Judge";
        public const string RegistrarRole = "Registrar";
        public const string ClerkRole = "Clerk";
        public const string PublicRole = "Public";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CourtDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            CourtDbContext context,
            ILogger<DashboardController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = AdminRole)]
        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error");
            }
        }

        [Authorize(Roles = JudgeRole)]
        public async Task<IActionResult> JudgeDashboard()
        {
            try
            {
                var judgeId = _userManager.GetUserId(User);
                _logger.LogInformation("Current Judge ID: {JudgeId}", judgeId);

                if (string.IsNullOrEmpty(judgeId))
                {
                    _logger.LogWarning("Judge ID is null - redirecting to login");
                    return RedirectToAction("Login", "Account");
                }

                var model = new JudgeDashboardViewModel
                {
                    TotalCases = await _context.Cases.CountAsync(c => c.AssignedJudgeId == judgeId),
                    PendingCases = await _context.Cases.CountAsync(c => c.AssignedJudgeId == judgeId && c.Status == "Pending"),
                    RecentCases = await _context.Cases
                        .Include(c => c.AssignedJudge)
                        .Where(c => c.AssignedJudgeId == judgeId && c.Status != "Pending" && c.Status != "Declined")
                        .OrderByDescending(c => c.RegistrationDate)
                        .Take(5)
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading judge dashboard");
                return View("Error");
            }
         }
          [HttpPost]
          [Authorize(Roles = JudgeRole + "," + RegistrarRole)]
           [ValidateAntiForgeryToken]
          public async Task<IActionResult> UpdateStatus(int id, string newStatus)
            {
            var caseToUpdate = await _context.Cases
                .Include(c => c.AssignedJudge)
                .FirstOrDefaultAsync(c => c.CaseID == id);

            if (caseToUpdate == null)
            {
                return NotFound();
            }

            // Verify judge is assigned to this case
            var currentUserId = _userManager.GetUserId(User);
            if (caseToUpdate.AssignedJudgeId != currentUserId)
            {
                return Forbid();
            }

            // Validate allowed status transitions
            var allowedStatuses = new[] { "Opened", "Closed", "Pending" };
            if (!allowedStatuses.Contains(newStatus))
            {
                ModelState.AddModelError("", "Invalid status selection");
                return RedirectToAction("Details", new { id });
            }

            caseToUpdate.Status = newStatus;
            caseToUpdate.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }
       [Authorize(Roles = JudgeRole + "," + ClerkRole)]
public async Task<IActionResult> ViewCases(
    string searchString,
    string searchType = "CaseNumber",
    int page = 1,
    int pageSize = 10)
{
    try
    {
        var currentUserId = _userManager.GetUserId(User);

        IQueryable<Case> query;

        if (User.IsInRole(ClerkRole))
        {
            // Clerk: See all cases except pending
            query = _context.Cases
                .Include(c => c.AssignedJudge)
                .Where(c => c.Status != "Pending");
        }
        else
        {
            // Judge: See only assigned cases except pending
            query = _context.Cases
                .Include(c => c.AssignedJudge)
                .Where(c => c.AssignedJudgeId == currentUserId && c.Status != "Pending");
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.Trim().ToLower();

            query = searchType switch
            {
                "CaseNumber" => query.Where(c => c.CaseNumber.ToLower().Contains(searchString)),
                "Title" => query.Where(c => c.Title.ToLower().Contains(searchString)),
                "Plaintiff" => query.Where(c => c.PlaintiffName.ToLower().Contains(searchString)),
                "Defendant" => query.Where(c => c.DefendantName.ToLower().Contains(searchString)),
                _ => query
            };
        }

        var totalItems = await query.CountAsync();

        var cases = await query
            .OrderByDescending(c => c.RegistrationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View("ViewCases", new CaseSearchViewModel
        {
            Cases = cases,
            SearchString = searchString,
            SearchType = searchType,
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error viewing cases");
        return View("Error");
    }
}

        [Authorize(Roles = JudgeRole)]
        public async Task<IActionResult> ViewPendingCases(
            string searchString,
            string searchType = "CaseNumber",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var query = _context.Cases
                    .Include(c => c.AssignedJudge)
                    .Where(c => c.AssignedJudgeId == currentUserId && c.Status == "Pending");

                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim().ToLower();
                    query = searchType switch
                    {
                        "CaseNumber" => query.Where(c => c.CaseNumber.ToLower().Contains(searchString)),
                        "Title" => query.Where(c => c.Title.ToLower().Contains(searchString)),
                        "Plaintiff" => query.Where(c => c.PlaintiffName.ToLower().Contains(searchString)),
                        "Defendant" => query.Where(c => c.DefendantName.ToLower().Contains(searchString)),
                        _ => query
                    };
                }

                var totalItems = await query.CountAsync();
                var cases = await query
                    .OrderByDescending(c => c.RegistrationDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return View("ViewPendingCases", new CaseSearchViewModel
                {
                    Cases = cases,
                    SearchString = searchString,
                    SearchType = searchType,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing pending cases");
                return View("Error");
            }
        }

        [Authorize(Roles = RegistrarRole)]
        public async Task<IActionResult> RegistrarDashboard()
        {
            try
            {
                var model = new DashboardViewModel
                {
                    TodaysCaseCount = await _context.Cases
                        .CountAsync(c => c.RegistrationDate.Date == DateTime.UtcNow.Date),
                    PendingCaseCount = await _context.Cases
                        .CountAsync(c => c.Status == "Pending"),
                    RecentCases = await _context.Cases
                        .Include(c => c.AssignedJudge)
                        .OrderByDescending(c => c.RegistrationDate)
                        .Take(5)
                        .ToListAsync()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading registrar dashboard");
                return View("Error");
            }
        }

        [Authorize(Roles = ClerkRole)]
public async Task<IActionResult> ClerkDashboard()
{
    try
    {
       var model = new ClerkDashboardViewModel
      {
       TotalCases = await _context.Cases.CountAsync(),
       PendingCases = await _context.Cases.CountAsync(c => c.Status == "Pending"),
       TodayRegisteredCases = await _context.Cases
        .CountAsync(c => c.RegistrationDate.Date == DateTime.UtcNow.Date),
       RecentCases = await _context.Cases
        .Include(c => c.AssignedJudge)
        .Where(c => c.Status != "Pending" && c.Status != "Declined")
        .OrderByDescending(c => c.RegistrationDate)
        .Take(5)
        .ToListAsync()
      };


        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading clerk dashboard");
        return View("Error");
    }
}
}
}