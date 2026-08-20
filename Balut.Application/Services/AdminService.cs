using Balut.Application.Interfaces;
using Balut.Application.ViewModels;
using Balut.Data.Context;
using Balut.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Balut.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditLogService _auditLog;

        public AdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICurrentUserService currentUser,
            IAuditLogService auditLog)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUser = currentUser;
            _auditLog = auditLog;
        }

        public async Task<PagedResult<AdminUserViewModel>> GetUsersPagedAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.UserName!.Contains(s) || u.FirstName.Contains(s) || u.LastName.Contains(s));
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(u => new AdminUserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt
                }).ToListAsync();

            var userIds = items.Select(i => i.Id).ToList();

            var userRoles = await _context.UserRoles
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name! })
                .Where(x => userIds.Contains(x.UserId))
                .ToListAsync();

            foreach (var item in items)
            {
                item.Roles = userRoles.Where(x => x.UserId == item.Id).Select(x => x.RoleName).ToList();
            }

            return new PagedResult<AdminUserViewModel> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<AjaxResult> SetUserRolesAsync(ChangeUserRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return new AjaxResult { Success = false, Message = "کاربر یافت نشد." };

            // محافظت: SuperAdmin نمی‌تواند نقش خودش را حذف کند
            if (user.Id == _currentUser.UserId && !request.Roles.Contains("SuperAdmin"))
                return new AjaxResult { Success = false, Message = "نمی‌توانید نقش SuperAdmin خود را حذف کنید." };

            foreach (var role in request.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    return new AjaxResult { Success = false, Message = $"نقش {role} وجود ندارد." };
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (request.Roles.Count > 0)
                await _userManager.AddToRolesAsync(user, request.Roles);

            await _auditLog.LogAsync("RoleChanged", "User", user.Id,
                $"نقش‌های {user.UserName} تغییر کرد به: {string.Join(", ", request.Roles)}");

            return new AjaxResult { Success = true, Message = "نقش‌ها با موفقیت اعمال شد." };
        }

        public async Task<AjaxResult> ToggleUserStatusAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new AjaxResult { Success = false, Message = "کاربر یافت نشد." };

            if (user.Id == _currentUser.UserId)
                return new AjaxResult { Success = false, Message = "نمی‌توانید حساب خود را غیرفعال کنید." };

            if (user.Status == 1)
            {
                user.Status = 0;
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue; // مسدودسازی واقعی ورود
            }
            else
            {
                user.Status = 1;
                user.LockoutEnd = null;
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync("ToggleStatus", "User", user.Id,
                $"وضعیت {user.UserName} به {(user.Status == 1 ? "فعال" : "غیرفعال")} تغییر کرد");

            return new AjaxResult { Success = true, Message = user.Status == 1 ? "کاربر فعال شد." : "کاربر غیرفعال و مسدود شد." };
        }

        public async Task<AjaxResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return new AjaxResult { Success = false, Message = "کاربر یافت نشد." };

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                return new AjaxResult { Success = false, Message = "رمز عبور باید حداقل 8 کاراکتر باشد." };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
                return new AjaxResult { Success = false, Message = "خطا: " + string.Join(", ", result.Errors.Select(e => e.Description)) };

            await _auditLog.LogAsync("ResetPassword", "User", user.Id, $"رمز عبور {user.UserName} ریست شد");

            return new AjaxResult { Success = true, Message = "رمز عبور با موفقیت تغییر کرد." };
        }

        public async Task<List<RoleViewModel>> GetRolesAsync()
        {
            return await _context.Roles.AsNoTracking()
                .Select(r => new RoleViewModel
                {
                    Name = r.Name!,
                    UserCount = _context.UserRoles.Count(ur => ur.RoleId == r.Id)
                })
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<AjaxResult> CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return new AjaxResult { Success = false, Message = "نام نقش الزامی است." };

            if (await _roleManager.RoleExistsAsync(roleName.Trim()))
                return new AjaxResult { Success = false, Message = "این نقش قبلاً ساخته شده است." };

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
            if (!result.Succeeded)
                return new AjaxResult { Success = false, Message = "خطا: " + string.Join(", ", result.Errors.Select(e => e.Description)) };

            await _auditLog.LogAsync("Create", "Role", roleName, $"نقش {roleName} ایجاد شد");

            return new AjaxResult { Success = true, Message = "نقش با موفقیت ساخته شد." };
        }

        public async Task<PagedResult<AuditLogViewModel>> GetAuditLogsPagedAsync(string? search, int pageNumber, int pageSize)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(a => a.Action.Contains(s) || a.EntityType.Contains(s) || (a.Details != null && a.Details.Contains(s)));
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(a => new AuditLogViewModel
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = _context.Users.Where(u => u.Id == a.UserId).Select(u => u.UserName).FirstOrDefault(),
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    IpAddress = a.IpAddress,
                    Details = a.Details,
                    Timestamp = a.Timestamp
                }).ToListAsync();

            return new PagedResult<AuditLogViewModel> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }
    }
}