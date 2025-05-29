using CourtCaseTrackingSystem.Models;
using CourtCaseTrackingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new RegisterViewModel
        {
            RoleOptions = await _roleManager.Roles
            .Select(r => new SelectListItem(r.Name, r.Name))
            .ToListAsync()

        };
        return View(model);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.RoleOptions = await _roleManager.Roles
            .Select(r => new SelectListItem(r.Name, r.Name))
            .ToListAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            FirstName = model.FirstName,  // Add this
            SecondName = model.SecondName, // Add this
            UserName = model.UserName,
            Email = model.Email,
            Status = "Active"
        };

        // Create user
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user); // This actually saves EmailConfirmed = true
        }
        if (!result.Succeeded)
        {
            AddErrors(result);
            model.RoleOptions = await GetRoleOptions();
            return View(model);
        }

        // Handle role assignment
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            // Ensure role exists (case-insensitive check)
            var normalizedRoleName = model.SelectedRole.ToUpper();
            var roleExists = await _roleManager.Roles
                .AnyAsync(r => r.NormalizedName == normalizedRoleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(model.SelectedRole));
            }

            // Assign role
            var roleResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user); // Rollback user creation
                AddErrors(roleResult);
                model.RoleOptions = await GetRoleOptions();
                return View(model);
            }
        }

        TempData["SuccessMessage"] = "User created successfully!";
        return RedirectToAction("AdminDashboard", "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);

        // Handle potential null email from legacy data
        var email = user.Email ?? "default@example.com";

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = email,
            UserName = user.UserName ?? string.Empty,
            Status = user.Status,
            SelectedRole = roles.FirstOrDefault() ?? string.Empty,
            RoleOptions = await GetRoleOptions()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            model.RoleOptions = await GetRoleOptions();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        // Update user properties
        user.Email = model.Email;
        user.UserName = model.UserName;
        user.Status = model.Status;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddErrors(updateResult);
            model.RoleOptions = await GetRoleOptions();
            return View(model);
        }

        // Handle role changes
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            // Ensure role exists
            var normalizedRoleName = model.SelectedRole.ToUpper();
            var roleExists = await _roleManager.Roles
                .AnyAsync(r => r.NormalizedName == normalizedRoleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(model.SelectedRole));
            }

            await _userManager.AddToRoleAsync(user, model.SelectedRole);
        }

        TempData["SuccessMessage"] = "User updated successfully!";
        return RedirectToAction("AdminDashboard", "Dashboard");
    }

    private async Task<List<SelectListItem>> GetRoleOptions()
    {
        return await _roleManager.Roles
            .Select(r => new SelectListItem(r.Name, r.Name))
            .ToListAsync();
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found!";
            return RedirectToAction("AdminDashboard", "Dashboard");
        }

        user.Status = "Inactive";
        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "User deactivated successfully!";

        return RedirectToAction("AdminDashboard", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found!";
            return RedirectToAction("AdminDashboard", "Dashboard");
        }

        user.Status = "Active";
        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "User activated successfully!";

        return RedirectToAction("AdminDashboard", "Dashboard");
    }
}