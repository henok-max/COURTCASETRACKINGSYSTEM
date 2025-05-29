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
        private readonly IEmailSender _emailSender;
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;

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

            var user = await _userManager.FindByNameAsync(model.UsernameOrEmail);
            if (user == null && model.UsernameOrEmail.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(model.UsernameOrEmail);

            }
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
                user.UserName!, // ✅ use resolved username from the found user
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false); // Enable lockout

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account locked: {Username}", user.UserName);
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
            return RedirectToAction("Login", "Account");
        }




        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Do not reveal that the user does not exist or is not confirmed
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                model.Email,
                "Reset Password",
                $"Reset your password by clicking here: <a href='{callbackUrl}'>Reset Password</a>");

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View(); // You can display a simple message here
        }
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don’t reveal user existence
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        return View(user);
    }





    }

}