using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourtCaseTrackingSystem.Models;
using CourtCaseTrackingSystem.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourtCaseTrackingSystem.ViewModels;
using CourtCaseTrackingSystem.Controllers;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Newtonsoft.Json;

public class CaseController : Controller
{
    public const string AdminRole = "Admin";
    public const string JudgeRole = "Judge";
    public const string RegistrarRole = "Registrar";
    public const string ClerkRole = "Clerk";
    public const string PublicRole = "Public";
    private readonly CourtDbContext _context;

    private readonly JudgeAssignmentService _judgeService;
    private readonly FileStorageService _fileService;
    private readonly ILogger<CaseController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager; // Add this


    public CaseController(
        CourtDbContext context,
                UserManager<ApplicationUser> userManager, // Add this parameter

        JudgeAssignmentService judgeService,
        FileStorageService fileService,
        ILogger<CaseController> logger,
                IWebHostEnvironment env) // Add this

    {
        _context = context;
        _userManager = userManager; // Initialize here

        _judgeService = judgeService;
        _fileService = fileService;
        _logger = logger;
        _env = env;

    }
    [HttpGet]
    [Authorize(Roles = JudgeRole)]
    [IgnoreAntiforgeryToken]

    public IActionResult DownloadDocument(string filePath, bool download = false)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }

            var safePath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));

            if (!System.IO.File.Exists(safePath))
            {
                return NotFound();
            }

            // For PDFs: Open in browser, others download
            var contentType = GetContentType(safePath);
            var fileName = Path.GetFileName(safePath);

            return PhysicalFile(
                safePath,
                contentType,
                download ? fileName : null,  // Only set filename to force download
                !download                     // Enable inline display for PDFs
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File download error");
            return StatusCode(500);
        }
    }
    private string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
    [HttpGet]
    public IActionResult Register()
    {
        try
        {
            var model = new CaseRegistrationViewModel
            {
                Title = string.Empty,
                PlaintiffName = string.Empty,
                DefendantName = string.Empty,
                Wereda = string.Empty,
                City = string.Empty,
                CaseTypeOptions = GetCaseTypeOptions()
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading registration form");
            return RedirectToAction("Error", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CaseRegistrationViewModel model)
    {
        model.CaseTypeOptions = GetCaseTypeOptions();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {

            // Remove the document null check
            string? documentPath = null;
            if (model.Document != null)
            {
                   documentPath = await _fileService.SaveDocument(model.Document, DocumentType.Case);
            }

            var assignmentResult = await _judgeService.AssignJudge();
            if (!assignmentResult.Succeeded)
            {
                assignmentResult.Errors.ForEach(e => ModelState.AddModelError("", e));
                return View(model);
            }

            var judge = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == assignmentResult.AssignedJudgeId);

            if (judge == null)
            {
                ModelState.AddModelError("", "Assigned judge not found");
                return View(model);
            }

            var caseEntry = new Case
            {
                CaseNumber = GenerateCaseNumber(),
                Title = model.Title.Trim(),
                CaseType = model.SelectedCaseType,
                Wereda = model.Wereda.Trim(),
                City = model.City.Trim(),
                PlaintiffName = model.PlaintiffName.Trim(),
                DefendantName = model.DefendantName.Trim(),
                Description = model.Description?.Trim(),
                Status = "Pending",
                RegistrationDate = DateTime.UtcNow,
                DocumentPath = documentPath,
                AssignedJudgeId = assignmentResult.AssignedJudgeId,
                AssignedJudge = judge
            };

            await _context.Cases.AddAsync(caseEntry);
            await _context.SaveChangesAsync();

            TempData["CaseNumber"] = caseEntry.CaseNumber;
            return RedirectToAction("Confirmation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering case");
            ModelState.AddModelError("", "An error occurred while processing your request");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Confirmation()
    {
        var caseNumber = TempData["CaseNumber"]?.ToString();
        return string.IsNullOrEmpty(caseNumber)
            ? RedirectToAction("Register")
            : View(new CaseConfirmationViewModel { CaseNumber = caseNumber });
    }

    private string GenerateCaseNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var sequence = _context.Cases
            .Count(c => c.RegistrationDate.Date == DateTime.UtcNow.Date) + 1;
        return $"CASE-{datePart}-{sequence:0000}";
    }

    private List<SelectListItem> GetCaseTypeOptions()
    {
        return Enum.GetValues(typeof(CaseType))
            .Cast<CaseType>()
            .Select(ct => new SelectListItem
            {
                Value = ct.ToString(),
                Text = ct.ToString()
            }).ToList();
    }
    [Authorize(Roles = JudgeRole)]
    public async Task<IActionResult> ViewCases(
      string searchString,
      string searchType = "CaseNumber",
      int page = 1,
      int pageSize = 10)
    {
        try
        {
            var query = _context.Cases
                .Include(c => c.AssignedJudge)
                .AsQueryable();

            // Search Logic
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();

                query = searchType switch
                {
                    "CaseNumber" => query.Where(c => c.CaseNumber.ToLower().Contains(searchString)),
                    "Title" => query.Where(c => c.Title.ToLower().Contains(searchString)),
                    "Plaintiff" => query.Where(c => c.PlaintiffName.ToLower().Contains(searchString)),
                    "Defendant" => query.Where(c => c.DefendantName.ToLower().Contains(searchString)),
                    "Status" => query.Where(c => c.Status.ToLower().Contains(searchString)),
                    _ => query
                };
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var cases = await query
                .OrderByDescending(c => c.RegistrationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new CaseSearchViewModel
            {
                Cases = cases,
                SearchString = searchString,
                SearchType = searchType,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cases");
            return View("Error");
        }
    } 
        [Authorize(Roles = JudgeRole + "," + RegistrarRole)]
    public async Task<IActionResult> Details(int id)
    {
        var caseItem = await _context.Cases
            .Include(c => c.AssignedJudge)
            .FirstOrDefaultAsync(c => c.CaseID == id);

        if (caseItem == null)
        {
            return NotFound();
        }

        // Allow Admins/Registrars/Clerks to bypass judge check
        if (User.IsInRole(RegistrarRole) && !User.IsInRole(AdminRole))
        {
            var currentUserId = _userManager.GetUserId(User);
            if (caseItem.AssignedJudgeId != currentUserId)
            {
                return Forbid();
            }
        }

        return View(caseItem);
    }
    [HttpPost]
    [Authorize(Roles = JudgeRole)]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ProcessDecision([FromBody] CaseDecisionModel model)
    {
        try
        {
            var caseItem = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseID == model.CaseId);

            if (caseItem == null)
            {
                _logger.LogWarning("Case {CaseId} not found", model.CaseId);
                return NotFound();
            }

            // Verify current user is assigned judge
            var currentUserId = _userManager.GetUserId(User);
            if (caseItem.AssignedJudgeId != currentUserId)
            {
                _logger.LogWarning("Unauthorized access attempt by {UserId} on case {CaseId}", currentUserId, model.CaseId);
                return Forbid();
            }

            // Validate decision
            if (string.IsNullOrWhiteSpace(model.Decision))
            {
                return BadRequest("Decision type is required");
            }

            caseItem.DecisionComments = model.Comments;

            if (model.Decision.Equals("accept", StringComparison.OrdinalIgnoreCase))
            {
                if (!model.SummonDate.HasValue)
                {
                    return BadRequest("Summon date is required for acceptance");
                }

                if (model.SummonDate.Value < DateTime.Today)
                {
                    return BadRequest("Cannot set summon date in the past");
                }

                // Generate and save summon letter
                var pdfContent = GenerateSummonPdfContent(caseItem, model.SummonDate.Value);
                caseItem.SummonLetterPath = _fileService.SaveSummonLetter(
                    pdfContent,
                    $"Summon_{caseItem.CaseNumber}.pdf"
                );

                caseItem.Status = "Accepted";
                caseItem.SummonDate = model.SummonDate.Value;
            }
            else
            {
                caseItem.Status = "Declined";
                caseItem.SummonDate = null;
                caseItem.SummonLetterPath = null;
            }

            caseItem.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing decision for case {CaseId}", model.CaseId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    private byte[] GenerateSummonPdfContent(Case caseItem, DateTime summonDate)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Header()
                    .AlignCenter()
                    .Text($"Court Summon Letter")
                    .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken3);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Item().Text($"Case Number: {caseItem.CaseNumber}");
                        col.Item().Text($"Title: {caseItem.Title}");
                        col.Item().Text($"Summon Date: {summonDate:dd MMMM yyyy}");
                        col.Item().Text($"Issuing Judge: {User.Identity?.Name}");
                        col.Item().Text($"Registration Date: {caseItem.RegistrationDate:dd MMMM yyyy}");

                        col.Item().PaddingTop(20).Text("Instructions:");
                        col.Item().Text("1. Both parties must appear in person");
                        col.Item().Text("2. Bring all relevant documentation");
                        col.Item().Text("3. Arrive 30 minutes before scheduled time");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        }).GeneratePdf(); // This returns byte[] directly
    }
    public class CaseDecisionModel
    {
        public int CaseId { get; set; }
        public required string Decision { get; set; }
        public DateTime? SummonDate { get; set; }
        public string? Comments { get; set; }
    }
        [HttpPost]
        [Authorize(Roles = JudgeRole + "," + ClerkRole)]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> UploadDefenseDocument(int caseId, IFormFile file)
          {
           try
           {
        var caseItem = await _context.Cases.FindAsync(caseId);
        if (caseItem == null) return NotFound();

        // Validate file
        if (file.Length == 0 || file.Length > 10 * 1024 * 1024)
        {
            ModelState.AddModelError("", "File must be between 1 byte and 10MB");
            return RedirectToAction("Details", new { id = caseId });
        }

        // Save document
        var defenseDocPath = await _fileService.SaveDocument(file, DocumentType.Defense);
        
        // Update case record
        caseItem.DefenseDocumentPath = defenseDocPath;
        caseItem.DefenseDocumentDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = caseId });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error uploading defense document");
        ModelState.AddModelError("", "Error uploading document");
        return RedirectToAction("Details", new { id = caseId });
    }
}
        [HttpPost]
        [Authorize(Roles = JudgeRole + "," + ClerkRole)]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> UploadWitnessDocument(int caseId, IFormFile file)
          {
           try
           {
        var caseItem = await _context.Cases.FindAsync(caseId);
        if (caseItem == null) return NotFound();

        // Validate file
        if (file.Length == 0 || file.Length > 10 * 1024 * 1024)
        {
            ModelState.AddModelError("", "File must be between 1 byte and 10MB");
            return RedirectToAction("Details", new { id = caseId });
        }

        // Save document
        var witnessDocPath = await _fileService.SaveDocument(file, DocumentType.Witness);
        
        // Update case record
        caseItem.WitnessDocumentPath = witnessDocPath;
        caseItem.WitnessDocumentDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = caseId });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error uploading witness document");
        ModelState.AddModelError("", "Error uploading document");
        return RedirectToAction("Details", new { id = caseId });
    }
}
[HttpPost]
[Authorize(Roles = "Judge")]
public async Task<IActionResult> SetHearingDate(int id, DateTime hearingDate)
{
    var caseToUpdate = await _context.Cases.FindAsync(id);

    if (caseToUpdate == null)
    {
        return NotFound();
    }

    caseToUpdate.HearingDate = hearingDate;
    caseToUpdate.Status = "Opened"; // update status
    _context.Update(caseToUpdate);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Details), new { id });
   }
[HttpPost]
[Authorize(Roles = "Judge,Admin")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateCaseDetails(Case model)
{
    try
    {
        var caseItem = await _context.Cases.FindAsync(model.CaseID);
        if (caseItem == null) return NotFound();

        // Verify user permissions
        if (!User.IsInRole("Admin") && User.IsInRole("Judge") && 
            caseItem.AssignedJudgeId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        // Update basic fields
        caseItem.Status = model.Status;
        caseItem.AppointmentDate = model.AppointmentDate;
        caseItem.Plea = model.Plea;

        // Handle judgment history
        var judgments = string.IsNullOrEmpty(caseItem.JudgmentHistory) 
            ? new List<CaseHistoryEntry>()
            : JsonConvert.DeserializeObject<List<CaseHistoryEntry>>(caseItem.JudgmentHistory);

        if (!string.IsNullOrWhiteSpace(model.NewJudgment))
        {
            judgments.Add(new CaseHistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Text = model.NewJudgment
            });
            caseItem.JudgmentHistory = JsonConvert.SerializeObject(judgments);
        }

        // Handle decree history
        var decrees = string.IsNullOrEmpty(caseItem.DecreeHistory) 
            ? new List<CaseHistoryEntry>()
            : JsonConvert.DeserializeObject<List<CaseHistoryEntry>>(caseItem.DecreeHistory);

        if (!string.IsNullOrWhiteSpace(model.NewDecree))
        {
            decrees.Add(new CaseHistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Text = model.NewDecree
            });
            caseItem.DecreeHistory = JsonConvert.SerializeObject(decrees);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = model.CaseID });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating case details");
        ModelState.AddModelError("", "An error occurred while saving updates");
        return RedirectToAction("Details", new { id = model.CaseID });
    }
}


}
