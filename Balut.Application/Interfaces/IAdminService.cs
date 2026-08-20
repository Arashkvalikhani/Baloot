using Balut.Application.ViewModels;

namespace Balut.Application.Interfaces
{
    public interface IAdminService
    {
        Task<PagedResult<AdminUserViewModel>> GetUsersPagedAsync(string? search, int pageNumber, int pageSize);
        Task<AjaxResult> SetUserRolesAsync(ChangeUserRolesRequest request);
        Task<AjaxResult> ToggleUserStatusAsync(string userId);
        Task<AjaxResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<List<RoleViewModel>> GetRolesAsync();
        Task<AjaxResult> CreateRoleAsync(string roleName);
        Task<PagedResult<AuditLogViewModel>> GetAuditLogsPagedAsync(string? search, int pageNumber, int pageSize);
    }
}