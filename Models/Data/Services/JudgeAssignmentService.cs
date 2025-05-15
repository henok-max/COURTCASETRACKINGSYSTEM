using Microsoft.AspNetCore.Identity;
using CourtCaseTrackingSystem.Models;
using CourtCaseTrackingSystem.Data;

public class JudgeAssignmentService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CourtDbContext _context;
    private readonly ILogger<JudgeAssignmentService> _logger;

    public JudgeAssignmentService(
        UserManager<ApplicationUser> userManager,
        CourtDbContext context,
        ILogger<JudgeAssignmentService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<AssignmentResult> AssignJudge()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get all active judges
            var judges = await _userManager.GetUsersInRoleAsync("Judge");
            var compatibleJudges = judges?
                .Where(j => j.Status == "Active")
                .OrderBy(j => j.ActiveCaseCount)
                .ThenBy(j => j.LastAssignmentDate ?? DateTime.MinValue)
                .ToList();

            if (compatibleJudges?.Count == 0)
            {
                return new AssignmentResult
                {
                    Succeeded = false,
                    Errors = { "No available judges" }
                };
            }

            var selectedJudge = compatibleJudges!.First();
            selectedJudge.ActiveCaseCount++;
            selectedJudge.LastAssignmentDate = DateTime.UtcNow;

            _context.Users.Update(selectedJudge);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new AssignmentResult
            {
                Succeeded = true,
                AssignedJudgeId = selectedJudge.Id
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Judge assignment failed");
            return new AssignmentResult
            {
                Succeeded = false,
                Errors = { "Assignment error" }
            };
        }
    }
}

public class AssignmentResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    public string AssignedJudgeId { get; set; } = string.Empty;
}