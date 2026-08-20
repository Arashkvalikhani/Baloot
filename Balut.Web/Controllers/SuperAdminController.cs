using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly IAdminService _adminService;

        public SuperAdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public IActionResult Index() => RedirectToAction(nameof(Users));

        public IActionResult Users() => View();

        public IActionResult Roles() => View();

        public IActionResult AuditLogs() => View();

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Json(await _adminService.GetUsersPagedAsync(search, pageNumber, pageSize));

        [HttpGet]
        public async Task<IActionResult> GetRoles()
            => Json(await _adminService.GetRolesAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRoles([FromBody] ChangeUserRolesRequest request)
            => Json(await _adminService.SetUserRolesAsync(request));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus([FromBody] UserIdRequest request)
            => Json(await _adminService.ToggleUserStatusAsync(request.UserId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
            => Json(await _adminService.ResetPasswordAsync(request));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
            => Json(await _adminService.CreateRoleAsync(request.RoleName));

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
            => Json(await _adminService.GetAuditLogsPagedAsync(search, pageNumber, pageSize));
    }
}