using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLog;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLog)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLog = auditLog;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _auditLog.LogAsync("Login", "User", model.UserName, "ورود موفق به سیستم");

                var user = await _userManager.FindByNameAsync(model.UserName);
                var roles = await _userManager.GetRolesAsync(user!);
                return RedirectToRoles(roles);
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "حساب شما موقتاً قفل شده است. ۱۵ دقیقه دیگر تلاش کنید.");
            else
                ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است.");

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        private static IActionResult RedirectToRoles(IList<string> roles)
        {
            if (roles.Contains("SuperAdmin") || roles.Contains("Admin") || roles.Contains("Secretary"))
                return new RedirectToActionResult("Index", "Dashboard", null);

            if (roles.Contains("Teacher"))
                return new RedirectToActionResult("Index", "TeacherPanel", null);

            if (roles.Contains("Student"))
                return new RedirectToActionResult("Index", "StudentPanel", null);

            if (roles.Contains("Parent"))
                return new RedirectToActionResult("Index", "ParentPanel", null);

            return new RedirectToActionResult("Index", "Dashboard", null);
        }
    }
}