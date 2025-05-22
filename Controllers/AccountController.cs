using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CourtCaseTrackingSystem.Models;
using CourtCaseTrackingSystem.ViewModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System; // Added for StringComparer
using System.Collections.Generic; // Added for IList<string>
namespace CourtCaseTrackingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        // Role constants
        private const string AdminRole = "Admin";
        private const string JudgeRole = "Judge";
        private const string RegistrarRole = "Registrar";
        private const string ClerkRole = "Clerk";

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(model);
            }

            if (user.Status != "Active")
            {
                ModelState.AddModelError(string.Empty, "Account inactive");
                return View(model);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid password");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false); // Enable lockout

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account locked: {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "Account temporarily locked");
                return View(model);
            }

            if (result.Succeeded)
            {
                user.LastLoginDate = DateTime.UtcNow;
                var updateResult = await _userManager.UpdateAsync(user);

                IList<string> roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation(
                    "User {UserId} logged in with roles: {Roles}",
                    user.Id,
                    string.Join(", ", roles)
                );

                var (actionName, controllerName) = GetDashboardRoute(roles);
                _logger.LogInformation("Redirecting to: {Controller}/{Action}", controllerName, actionName); _logger.LogInformation("Redirecting to: {Controller}/{Action}", controllerName, actionName);
                return RedirectToAction(actionName, controllerName);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View(model);
        }

        private (string Action, string Controller) GetDashboardRoute(IList<string> roles)
        {
            if (roles.Contains(AdminRole, StringComparer.OrdinalIgnoreCase))
                return ("AdminDashboard", "Dashboard");
            if (roles.Contains(JudgeRole, StringComparer.OrdinalIgnoreCase))
                return ("JudgeDashboard", "Dashboard");
            if (roles.Contains(RegistrarRole, StringComparer.OrdinalIgnoreCase))
                return ("RegistrarDashboard", "Dashboard");
                if (roles.Contains(ClerkRole, StringComparer.OrdinalIgnoreCase))
                return ("ClerkDashboard", "Dashboard");
            return ("UserDashboard", "Dashboard");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login");
        }
    }
}